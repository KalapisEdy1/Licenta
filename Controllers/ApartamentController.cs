using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using Microsoft.AspNetCore.Authorization;
using AdministrareBlocMVC.ViewModels;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApartamentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApartamentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Apartamente (alege bloc)
        public async Task<IActionResult> Index()
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .ToListAsync();

            return View(blocuri);
        }

        public async Task<IActionResult> ListaPeBloc(int id, string? scara)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == id);
            if (bloc == null) return NotFound();

            ViewBag.Bloc = bloc;

            var scari = ParseScari(bloc.Scari);
            ViewBag.Scari = scari;
            ViewBag.ScaraSelectata = scara;

            var q = _context.Apartamente
                .Include(a => a.Bloc)
                .Include(a => a.Chirias) 
                .Where(a => a.BlocId == id);

            if (scari.Length > 0 && !string.IsNullOrWhiteSpace(scara))
            {
                q = q.Where(a => a.Scara == scara);
            }

            var apartamente = await q
                .OrderBy(a => a.Numar)
                .ToListAsync();

            return View(apartamente);
        }

        // GET: Apartament/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var apartament = await _context.Apartamente
                .Include(a => a.Bloc)
                .Include(a => a.Chirias) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartament == null) return NotFound();

            return View(apartament);
        }

        // GET: Apartament/Create
        public async Task<IActionResult> Create()
        {
            await PopulateBlocuriDropdownAsync();
            ViewBag.Scari = new SelectList(Enumerable.Empty<string>());

            return View(new ApartamentCreateEditViewModel());
        }

        // POST: Apartament/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApartamentCreateEditViewModel vm)
        {
            var apartament = new Apartament
            {
                Id = vm.Id,
                Numar = vm.Numar,
                Etaj = vm.Etaj,
                BlocId = vm.BlocId,
                Scara = vm.Scara,
                NumeProprietar = vm.NumeProprietar,
                NumarCamere = vm.NumarCamere,
                SuprafataMp = vm.SuprafataMp,
                TelefonProprietar = vm.TelefonProprietar,
                NrPersoane = vm.NrPersoane 
            };

            await ValidateScaraConditionalaAsync(apartament);

            if (ModelState.IsValid)
            {
                _context.Add(apartament);
                await _context.SaveChangesAsync(); 

                if (vm.AdaugaChirias &&
                    (!string.IsNullOrWhiteSpace(vm.NumeChirias) || !string.IsNullOrWhiteSpace(vm.TelefonChirias)))
                {
                    var chirias = new Chirias
                    {
                        ApartamentId = apartament.Id,
                        NumeChirias = vm.NumeChirias?.Trim(),
                        TelefonChirias = vm.TelefonChirias?.Trim()
                    };

                    _context.Chiriasi.Add(chirias);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateBlocuriDropdownAsync(vm.BlocId);
            ViewBag.Scari = await GetScariSelectListAsync(vm.BlocId, vm.Scara);

            return View(vm);
        }

        // GET: Apartament/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var apartament = await _context.Apartamente
                .Include(a => a.Chirias)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartament == null) return NotFound();

            var vm = new ApartamentCreateEditViewModel
            {
                Id = apartament.Id,
                Numar = apartament.Numar,
                Etaj = apartament.Etaj,
                BlocId = apartament.BlocId,
                Scara = apartament.Scara,
                NumeProprietar = apartament.NumeProprietar,
                NumarCamere = apartament.NumarCamere,
                SuprafataMp = apartament.SuprafataMp,
                TelefonProprietar = apartament.TelefonProprietar,
                NrPersoane = apartament.NrPersoane, // ✅ NOU

                AreChirias = apartament.Chirias != null,
                AdaugaChirias = apartament.Chirias != null,
                NumeChirias = apartament.Chirias?.NumeChirias,
                TelefonChirias = apartament.Chirias?.TelefonChirias
            };

            await PopulateBlocuriDropdownAsync(apartament.BlocId);
            ViewBag.Scari = await GetScariSelectListAsync(apartament.BlocId, apartament.Scara);

            return View(vm);
        }

        // POST: Apartament/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApartamentCreateEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            var apartament = await _context.Apartamente
                .Include(a => a.Chirias)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartament == null) return NotFound();

            apartament.Numar = vm.Numar;
            apartament.Etaj = vm.Etaj;
            apartament.BlocId = vm.BlocId;
            apartament.Scara = vm.Scara;
            apartament.NumeProprietar = vm.NumeProprietar;
            apartament.NumarCamere = vm.NumarCamere;
            apartament.SuprafataMp = vm.SuprafataMp;
            apartament.TelefonProprietar = vm.TelefonProprietar;
            apartament.NrPersoane = vm.NrPersoane; // ✅ NOU

            await ValidateScaraConditionalaAsync(apartament);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apartament);
                    await _context.SaveChangesAsync();

                    if (vm.StergeChirias && vm.StergeChiriasConfirmat)
                    {
                        var chirias = await _context.Chiriasi.FirstOrDefaultAsync(c => c.ApartamentId == apartament.Id);
                        if (chirias != null)
                        {
                            _context.Chiriasi.Remove(chirias);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var chiriasExistent = await _context.Chiriasi.FirstOrDefaultAsync(c => c.ApartamentId == apartament.Id);

                        if (chiriasExistent != null && !vm.AdaugaChirias)
                        {
                            
                        }
                        else
                        {
                            if (vm.AdaugaChirias &&
                                (!string.IsNullOrWhiteSpace(vm.NumeChirias) || !string.IsNullOrWhiteSpace(vm.TelefonChirias)))
                            {
                                if (chiriasExistent == null)
                                {
                                    chiriasExistent = new Chirias
                                    {
                                        ApartamentId = apartament.Id,
                                        NumeChirias = vm.NumeChirias?.Trim(),
                                        TelefonChirias = vm.TelefonChirias?.Trim()
                                    };
                                    _context.Chiriasi.Add(chiriasExistent);
                                }
                                else
                                {
                                    chiriasExistent.NumeChirias = vm.NumeChirias?.Trim();
                                    chiriasExistent.TelefonChirias = vm.TelefonChirias?.Trim();
                                    _context.Chiriasi.Update(chiriasExistent);
                                }

                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Apartamente.Any(e => e.Id == apartament.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateBlocuriDropdownAsync(vm.BlocId);
            ViewBag.Scari = await GetScariSelectListAsync(vm.BlocId, vm.Scara);

            return View(vm);
        }

        // GET: Apartament/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var apartament = await _context.Apartamente
                .Include(a => a.Bloc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartament == null) return NotFound();

            return View(apartament);
        }

        // POST: Apartament/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apartament = await _context.Apartamente.FindAsync(id);
            if (apartament != null)
            {
                var chirias = await _context.Chiriasi.FirstOrDefaultAsync(c => c.ApartamentId == id);
                if (chirias != null) _context.Chiriasi.Remove(chirias);

                _context.Apartamente.Remove(apartament);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetByBloc(int blocId)
        {
            var apartamente = await _context.Apartamente
                .Where(a => a.BlocId == blocId)
                .OrderBy(a => a.Numar)
                .Select(a => new
                {
                    id = a.Id,
                    numar = a.Numar,
                    scara = a.Scara
                })
                .ToListAsync();

            return Json(apartamente);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetScariByBloc(int blocId)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) return Json(Array.Empty<string>());

            var scari = ParseScari(bloc.Scari);
            return Json(scari);
        }

        // GET: /Apartament/Cautare?q=ion
        [HttpGet]
        public async Task<IActionResult> Cautare(string? q)
        {
            q = (q ?? "").Trim();

            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Query = "";
                return View(new List<Apartament>());
            }

            var qLower = q.ToLower();

            var rezultate = await _context.Apartamente
                .Include(a => a.Bloc)
                .Where(a =>
                    a.NumeProprietar != null &&
                    a.NumeProprietar.ToLower().Contains(qLower)
                )
                .OrderBy(a => a.Bloc!.Nume)
                .ThenBy(a => a.Scara)
                .ThenBy(a => a.Numar)
                .ToListAsync();

            ViewBag.Query = q;
            return View(rezultate);
        }

        private async Task PopulateBlocuriDropdownAsync(int? selectedBlocId = null)
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .Select(b => new { b.Id, Display = b.Nume + " (" + b.Adresa + ")" })
                .ToListAsync();

            ViewData["BlocId"] = new SelectList(blocuri, "Id", "Display", selectedBlocId);
        }

        private async Task<SelectList> GetScariSelectListAsync(int blocId, string? selectedScara)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            var scari = bloc == null ? Array.Empty<string>() : ParseScari(bloc.Scari);
            return new SelectList(scari, selectedScara);
        }

        private static string[] ParseScari(string? scariRaw)
        {
            if (string.IsNullOrWhiteSpace(scariRaw))
                return Array.Empty<string>();

            return scariRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private async Task ValidateScaraConditionalaAsync(Apartament apartament)
        {
            if (apartament.BlocId <= 0)
                return;

            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == apartament.BlocId);
            if (bloc == null)
            {
                ModelState.AddModelError(nameof(Apartament.BlocId), "Bloc invalid.");
                return;
            }

            var scari = ParseScari(bloc.Scari);

            if (scari.Length == 0)
            {
                apartament.Scara = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(apartament.Scara))
            {
                ModelState.AddModelError(nameof(Apartament.Scara), "Scara este obligatorie pentru acest bloc.");
                return;
            }

            bool ok = scari.Any(s => string.Equals(s, apartament.Scara.Trim(), StringComparison.OrdinalIgnoreCase));
            if (!ok)
                ModelState.AddModelError(nameof(Apartament.Scara), "Scara aleasa nu exista in lista blocului selectat.");
        }
    }
}
