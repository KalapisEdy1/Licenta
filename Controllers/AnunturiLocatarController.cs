using System.Linq;
using System.Threading.Tasks;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize]
    public class AnunturiLocatarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Locatar> _userManager;

        public AnunturiLocatarController(ApplicationDbContext context, UserManager<Locatar> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /AnunturiLocatar/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue)
            {
                ViewBag.Mesaj = "Nu ai un apartament asignat. Contacteaza administratorul.";
                return View(Enumerable.Empty<Anunt>());
            }

            var apartament = await _context.Apartamente
                .Include(a => a.Bloc)
                .FirstOrDefaultAsync(a => a.Id == user.ApartamentId.Value);

            if (apartament?.BlocId == null)
            {
                ViewBag.Mesaj = "Apartamentul nu este asociat unui bloc.";
                return View(Enumerable.Empty<Anunt>());
            }

            ViewBag.Bloc = apartament.Bloc;

            var scaraLocatar = (apartament.Scara ?? "").Trim();

            var q = _context.Anunturi
                .Include(a => a.Bloc)
                .Where(a => a.BlocId == apartament.BlocId);

          
            if (!string.IsNullOrWhiteSpace(scaraLocatar))
            {
                q = q.Where(a =>
                    string.IsNullOrWhiteSpace(a.Scara) ||
                    a.Scara == scaraLocatar
                );
            }

            var anunturi = await q
                .OrderByDescending(a => a.DataCreare)
                .ToListAsync();

            return View(anunturi);
        }
    }
}
