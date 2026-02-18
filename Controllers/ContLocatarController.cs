using System.Threading.Tasks;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using AdministrareBlocMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize]
    public class ContLocatarController : Controller
    {
        private readonly UserManager<Locatar> _userManager;
        private readonly ApplicationDbContext _context;

        public ContLocatarController(UserManager<Locatar> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /ContLocatar/ApartamentulMeu
        public async Task<IActionResult> ApartamentulMeu()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            Apartament? ap = null;

            if (user.ApartamentId.HasValue)
            {
                ap = await _context.Apartamente
                    .Include(a => a.Bloc)
                    .Include(a => a.Chirias)
                    .FirstOrDefaultAsync(a => a.Id == user.ApartamentId.Value);
            }

            var vm = new ApartamentulMeuViewModel
            {
                Email = user.Email,
                ApartamentId = user.ApartamentId,

                NrPersoane = ap?.NrPersoane ?? 0,

                NumarApartament = ap?.Numar,
                Etaj = ap?.Etaj,
                NumeBloc = ap?.Bloc?.Nume,

                Scara = ap?.Scara,

                NumeProprietar = ap?.NumeProprietar,

                NumarCamere = ap?.NumarCamere ?? 0,
                SuprafataMp = ap?.SuprafataMp ?? 0,
                TelefonProprietar = ap?.TelefonProprietar,

                NumeChirias = ap?.Chirias?.NumeChirias,
                TelefonChirias = ap?.Chirias?.TelefonChirias
            };

            return View(vm);
        }
    }
}
