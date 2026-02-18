using System;
using System.Linq;
using System.Threading.Tasks;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using AdministrareBlocMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministrareBlocMVC.Controllers
{
    public class AnunturiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnunturiController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Adauga()
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .ToListAsync();

            var blocDefault = blocuri.FirstOrDefault();
            if (blocDefault == null)
            {
                TempData["Msg"] = "Nu exista blocuri in sistem. Creeaza un bloc mai intai.";
                ViewBag.Blocuri = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Scari = new SelectList(Enumerable.Empty<SelectListItem>());
                return View(new AdaugaAnuntViewModel());
            }

            ViewBag.Blocuri = new SelectList(
                blocuri.Select(b => new
                {
                    b.Id,
                    Text = $"{b.Nume} - {b.Adresa}"
                }),
                "Id",
                "Text",
                blocDefault.Id
            );

            var scari = ParseScari(blocDefault.Scari);
            ViewBag.Scari = new SelectList(scari);

            return View(new AdaugaAnuntViewModel
            {
                BlocId = blocDefault.Id,
                Scara = null
            });
        }

        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetScariByBloc(int blocId)
        {
            var bloc = await _context.Blocuri
                .FirstOrDefaultAsync(b => b.Id == blocId);

            if (bloc == null)
                return Json(Array.Empty<string>());

            var scari = ParseScari(bloc.Scari);
            return Json(scari);
        }

     
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adauga(AdaugaAnuntViewModel vm)
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .ToListAsync();

            ViewBag.Blocuri = new SelectList(
                blocuri.Select(b => new
                {
                    b.Id,
                    Text = $"{b.Nume} - {b.Adresa}"
                }),
                "Id",
                "Text",
                vm.BlocId
            );

            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == vm.BlocId);
            var scari = bloc == null ? Array.Empty<string>() : ParseScari(bloc.Scari);

            ViewBag.Scari = new SelectList(scari);

            if (!ModelState.IsValid)
                return View(vm);

            string? scaraFinala = string.IsNullOrWhiteSpace(vm.Scara)
                ? null
                : vm.Scara.Trim();

            if (scari.Length == 0)
            {
                scaraFinala = null;
            }
            else if (scaraFinala != null)
            {
                bool exista = scari.Any(s =>
                    string.Equals(s, scaraFinala, StringComparison.OrdinalIgnoreCase));

                if (!exista) scaraFinala = null;
            }

            var anunt = new Anunt
            {
                BlocId = vm.BlocId,
                Mesaj = vm.Mesaj,
                Scara = scaraFinala
            };

            _context.Anunturi.Add(anunt);
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Anuntul a fost publicat.";
            return RedirectToAction(nameof(Adauga));
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
    }
}
