using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BlocController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlocController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bloc
        public async Task<IActionResult> Index()
        {
            return _context.Blocuri != null
                ? View(await _context.Blocuri.OrderBy(b => b.Nume).ToListAsync())
                : Problem("Entity set 'ApplicationDbContext.Blocuri' is null.");
        }

        // GET: Bloc/Create
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nume,Adresa,Scari")] Bloc bloc)
        {
            bloc.Scari = SeparaScari(bloc.Scari);

            if (!ModelState.IsValid)
                return View(bloc);

            _context.Add(bloc);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Bloc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Blocuri == null)
                return NotFound();

            var bloc = await _context.Blocuri.FindAsync(id);
            if (bloc == null)
                return NotFound();

            return View(bloc);
        }

        // POST: Bloc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nume,Adresa,Scari")] Bloc bloc)
        {
            if (id != bloc.Id)
                return NotFound();

            bloc.Scari = SeparaScari(bloc.Scari);

            if (!ModelState.IsValid)
                return View(bloc);

            try
            {
                _context.Update(bloc);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlocExists(bloc.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bloc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Blocuri == null)
                return NotFound();

            var bloc = await _context.Blocuri.FirstOrDefaultAsync(m => m.Id == id);
            if (bloc == null)
                return NotFound();

            return View(bloc);
        }

        // POST: Bloc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Blocuri == null)
                return Problem("Entity set 'ApplicationDbContext.Blocuri' is null.");

            var bloc = await _context.Blocuri.FindAsync(id);
            if (bloc != null)
            {
                _context.Blocuri.Remove(bloc);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlocExists(int id)
        {
            return (_context.Blocuri?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private static string? SeparaScari(string? scariRaw)
        {
            if (string.IsNullOrWhiteSpace(scariRaw))
                return null; // bloc fara scari

            var scari = scariRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (scari.Count == 0)
                return null;

            return string.Join(",", scari);
        }
    }
}
