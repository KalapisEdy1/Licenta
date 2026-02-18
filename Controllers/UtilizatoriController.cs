using System.Linq;
using System.Threading.Tasks;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using AdministrareBlocMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilizatoriController : Controller
    {
        private readonly UserManager<Locatar> _userManager;
        private readonly ApplicationDbContext _context;

        public UtilizatoriController(UserManager<Locatar> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string q)
        {
            var usersQuery = _userManager.Users
                .Include(u => u.Apartament)
                    .ThenInclude(a => a.Bloc)
                .Include(u => u.Apartament)
                    .ThenInclude(a => a.Chirias)
                .AsQueryable();

            var admini = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admini.Select(x => x.Id).ToList();
            usersQuery = usersQuery.Where(u => !adminIds.Contains(u.Id));

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.Trim().ToLower();
                usersQuery = usersQuery.Where(u => u.Email != null && u.Email.ToLower().Contains(qq));
            }

            var users = await usersQuery
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Assign(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.Users
                .Include(u => u.Apartament)
                    .ThenInclude(a => a.Bloc)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            ViewBag.ApartamentCurent = user.Apartament;

            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .Select(b => new
                {
                    b.Id,
                    Text = b.Nume + " (" + b.Adresa + ")"
                })
                .ToListAsync();

            ViewBag.Blocuri = new SelectList(blocuri, "Id", "Text", user.Apartament?.BlocId);

            var apartamente = await _context.Apartamente
                .Include(a => a.Bloc)
                .OrderBy(a => a.Bloc.Nume)
                .ThenBy(a => a.Scara)
                .ThenBy(a => a.Numar)
                .ToListAsync();

            ViewBag.Apartamente = new SelectList(
                apartamente.Select(a => new
                {
                    a.Id,
                    Text = $"{a.Bloc.Nume}{(string.IsNullOrWhiteSpace(a.Scara) ? "" : $" - Scara {a.Scara}")} ({a.Bloc.Adresa}) - Ap {a.Numar} (Etaj {a.Etaj})"
                }),
                "Id",
                "Text",
                user.ApartamentId
            );

            var vm = new AsignareApartamentViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                ApartamentId = user.ApartamentId,
                BlocId = user.Apartament?.BlocId,
                Scara = user.Apartament?.Scara
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AsignareApartamentViewModel vm)
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .Select(b => new
                {
                    b.Id,
                    Text = b.Nume + " (" + b.Adresa + ")"
                })
                .ToListAsync();

            ViewBag.Blocuri = new SelectList(blocuri, "Id", "Text", vm.BlocId);

            var apartamente = await _context.Apartamente
                .Include(a => a.Bloc)
                .OrderBy(a => a.Bloc.Nume)
                .ThenBy(a => a.Scara)
                .ThenBy(a => a.Numar)
                .ToListAsync();

            ViewBag.Apartamente = new SelectList(
                apartamente.Select(a => new
                {
                    a.Id,
                    Text = $"{a.Bloc.Nume}{(string.IsNullOrWhiteSpace(a.Scara) ? "" : $" - Scara {a.Scara}")} ({a.Bloc.Adresa}) - Ap {a.Numar} (Etaj {a.Etaj})"
                }),
                "Id",
                "Text",
                vm.ApartamentId
            );

            if (!vm.BlocId.HasValue || vm.BlocId.Value <= 0)
            {
                vm.ApartamentId = null;
                vm.Scara = null;
            }

            if (!ModelState.IsValid)
            {
                var userInvalid = await _userManager.Users
                    .Include(u => u.Apartament)
                        .ThenInclude(a => a.Bloc)
                    .FirstOrDefaultAsync(u => u.Id == vm.UserId);

                ViewBag.ApartamentCurent = userInvalid?.Apartament;
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
                return NotFound();

            user.ApartamentId = vm.ApartamentId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                var userErr = await _userManager.Users
                    .Include(u => u.Apartament)
                        .ThenInclude(a => a.Bloc)
                    .FirstOrDefaultAsync(u => u.Id == vm.UserId);

                ViewBag.ApartamentCurent = userErr?.Apartament;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetApartamenteByBlocScara(int blocId, string? scara)
        {
            var q = _context.Apartamente.Where(a => a.BlocId == blocId);

            if (!string.IsNullOrWhiteSpace(scara))
                q = q.Where(a => a.Scara == scara);

            var list = await q
                .OrderBy(a => a.Numar)
                .Select(a => new
                {
                    id = a.Id,
                    text = $"Ap {a.Numar} (Etaj {a.Etaj})"
                })
                .ToListAsync();

            return Json(list);
        }
    }
}
