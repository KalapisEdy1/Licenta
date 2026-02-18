using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize(Roles = "Locatar")]
    public class IntretinereLocatarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Locatar> _userManager;
        private readonly IWebHostEnvironment _env;

        public IntretinereLocatarController(ApplicationDbContext context, UserManager<Locatar> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /IntretinereLocatar
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue)
            {
                ViewBag.Mesaj = "Nu ai un apartament asignat. Contacteaza administratorul.";
                return View(Enumerable.Empty<Intretinere>());
            }

            var intretineri = await _context.Intretineri
                .Include(i => i.Apartament)
                    .ThenInclude(a => a.Bloc)
                .Where(i => i.ApartamentId == user.ApartamentId.Value)
                .OrderByDescending(i => i.An)
                .ThenByDescending(i => i.Luna)
                .ToListAsync();

            ViewBag.Apartament = intretineri.FirstOrDefault()?.Apartament;

            return View(intretineri);
        }

        // GET: /IntretinereLocatar/Detalii
        public async Task<IActionResult> Detalii(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var intretinere = await _context.Intretineri
                .Include(i => i.Apartament)
                    .ThenInclude(a => a.Bloc)
                .Include(i => i.Detalii)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intretinere == null) return NotFound();
            if (!user.ApartamentId.HasValue || intretinere.ApartamentId != user.ApartamentId.Value)
                return Forbid();

            return View(intretinere);
        }

        // GET: /IntretinereLocatar/Export
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(int an)
        {
            if (an < 2010)
            {
                TempData["Err"] = "An invalid. Trebuie sa fie >= 2010.";
                return RedirectToAction(nameof(Export));
            }

            return RedirectToAction(nameof(ExportExcelAn), new { an });
        }

        public async Task<IActionResult> ExportExcelAn(int an)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue)
                return Forbid();

            int apartamentId = user.ApartamentId.Value;

            var ap = await _context.Apartamente
                .Include(a => a.Bloc)
                .FirstOrDefaultAsync(a => a.Id == apartamentId);

            if (ap == null) return NotFound();

            var intretineri = await _context.Intretineri
                .Include(i => i.Detalii)
                .Where(i => i.ApartamentId == apartamentId && i.An == an)
                .OrderBy(i => i.Luna)
                .ToListAsync();

            using var wb = new XLWorkbook();

            var sumar = wb.Worksheets.Add("SUMAR");

            sumar.Cell(1, 1).Value = "Bloc";
            sumar.Cell(1, 2).Value = ap.Bloc?.Nume;

            sumar.Cell(2, 1).Value = "Adresa";
            sumar.Cell(2, 2).Value = ap.Bloc?.Adresa;

            sumar.Cell(3, 1).Value = "Scara";
            sumar.Cell(3, 2).Value = string.IsNullOrWhiteSpace(ap.Scara) ? "-" : ap.Scara;

            sumar.Cell(4, 1).Value = "Apartament";
            sumar.Cell(4, 2).Value = $"Ap {ap.Numar}, Etaj {ap.Etaj}";

            sumar.Cell(5, 1).Value = "An";
            sumar.Cell(5, 2).Value = an;

            int headerRow = 7;
            sumar.Cell(headerRow, 1).Value = "Luna";

            var tipuri = Enum.GetValues(typeof(TipCheltuiala))
                .Cast<TipCheltuiala>()
                .OrderBy(t => (int)t)
                .ToList();

            int col = 2;
            foreach (var t in tipuri)
            {
                sumar.Cell(headerRow, col).Value = t.ToString();
                col++;
            }

            int totalCol = col;
            sumar.Cell(headerRow, totalCol).Value = "Total";

            sumar.Range(headerRow, 1, headerRow, totalCol).Style.Font.Bold = true;

            int row = headerRow + 1;

            for (int luna = 1; luna <= 12; luna++)
            {
                sumar.Cell(row, 1).Value = luna;

                var intr = intretineri.FirstOrDefault(i => i.Luna == luna);

                decimal totalLuna = 0;
                int c = 2;

                foreach (var t in tipuri)
                {
                    decimal sumaTip = 0;

                    if (intr != null)
                    {
                        sumaTip = intr.Detalii
                            .Where(d => d.TipCheltuiala == t)
                            .Sum(d => d.Suma);
                    }

                    sumar.Cell(row, c).Value = sumaTip;
                    totalLuna += sumaTip;
                    c++;
                }

                sumar.Cell(row, totalCol).Value = $"{totalLuna} RON";

                row++;
            }

            sumar.Columns().AdjustToContents();

            foreach (var intr in intretineri)
            {
                var ws = wb.Worksheets.Add($"{intr.Luna:D2}-{intr.An}");

                ws.Cell(1, 1).Value = "Bloc:";
                ws.Cell(1, 2).Value = ap.Bloc?.Nume;

                ws.Cell(2, 1).Value = "Adresa:";
                ws.Cell(2, 2).Value = ap.Bloc?.Adresa;

                ws.Cell(3, 1).Value = "Scara:";
                ws.Cell(3, 2).Value = string.IsNullOrWhiteSpace(ap.Scara) ? "-" : ap.Scara;

                ws.Cell(4, 1).Value = "Apartament:";
                ws.Cell(4, 2).Value = ap.Numar;

                ws.Cell(5, 1).Value = "Etaj:";
                ws.Cell(5, 2).Value = ap.Etaj;

                ws.Cell(6, 1).Value = "Luna/An:";
                ws.Cell(6, 2).Value = $"{intr.Luna:D2}/{intr.An}";

                ws.Cell(7, 1).Value = "Data generare:";
                ws.Cell(7, 2).Value = intr.DataGenerare.ToString("dd.MM.yyyy HH:mm");

                int rr = 9;
                ws.Cell(rr, 1).Value = "Cheltuiala";
                ws.Cell(rr, 2).Value = "Suma";
                ws.Range(rr, 1, rr, 2).Style.Font.Bold = true;
                rr++;

                foreach (var d in intr.Detalii.OrderBy(x => x.Id))
                {
                    ws.Cell(rr, 1).Value = d.TipCheltuiala.ToString();
                    ws.Cell(rr, 2).Value = d.Suma;
                    rr++;
                }

                rr++;
                ws.Cell(rr, 1).Value = "TOTAL";
                ws.Cell(rr, 1).Style.Font.Bold = true;

                ws.Cell(rr, 2).Value = $"{intr.Total} RON";
                ws.Cell(rr, 2).Style.Font.Bold = true;

                ws.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            var scaraPart = string.IsNullOrWhiteSpace(ap.Scara) ? "" : $"_Scara{ap.Scara}";
            var fileName = $"Intretinere_{ap.Bloc?.Nume}{scaraPart}_Ap{ap.Numar}_{an}.xlsx";

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        public async Task<IActionResult> Statistica()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.ApartamentId.HasValue)
            {
                var ap = await _context.Apartamente
                    .Include(a => a.Bloc)
                    .FirstOrDefaultAsync(a => a.Id == user.ApartamentId.Value);

                ViewBag.Apartament = ap;
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> StatisticaLuna(int luna, int an)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue)
                return Json(new { labels = Array.Empty<string>(), values = Array.Empty<decimal>() });

            int apartamentId = user.ApartamentId.Value;

            var intretinere = await _context.Intretineri
                .Include(i => i.Detalii)
                .FirstOrDefaultAsync(i =>
                    i.ApartamentId == apartamentId &&
                    i.Luna == luna &&
                    i.An == an);

            if (intretinere == null)
                return Json(new { labels = Array.Empty<string>(), values = Array.Empty<decimal>() });

            var data = intretinere.Detalii
                .GroupBy(d => d.TipCheltuiala)
                .Select(g => new
                {
                    Label = g.Key.ToString(),
                    Total = g.Sum(x => x.Suma)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            return Json(new
            {
                labels = data.Select(x => x.Label),
                values = data.Select(x => x.Total)
            });
        }

        // GET: /IntretinereLocatar/StatisticaAn?an=2026
        [HttpGet]
        public async Task<IActionResult> StatisticaAn(int an)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue)
                return Json(new { labels = Array.Empty<string>(), totals = Array.Empty<decimal>() });

            if (an < 2010 || an > 2100)
                return Json(new { labels = Array.Empty<string>(), totals = Array.Empty<decimal>() });

            int apartamentId = user.ApartamentId.Value;

            // luam toate intretinerile din anul respectiv + detalii
            var lista = await _context.Intretineri
                .Include(i => i.Detalii)
                .Where(i => i.ApartamentId == apartamentId && i.An == an)
                .ToListAsync();

            // total pe fiecare luna 1..12
            decimal[] totals = new decimal[12];

            foreach (var intr in lista)
            {
                if (intr.Luna < 1 || intr.Luna > 12) continue;

                // mai sigur decat intr.Total (daca nu e recalculat)
                var suma = (intr.Detalii == null) ? 0m : intr.Detalii.Sum(d => d.Suma);
                totals[intr.Luna - 1] += suma;
            }

            string[] labels = new[]
            {
                "Ian", "Feb", "Mar", "Apr", "Mai", "Iun",
                "Iul", "Aug", "Sep", "Oct", "Noi", "Dec"
            };

            return Json(new
            {
                labels,
                totals
            });
        }

        [HttpGet]
        public async Task<IActionResult> Publicari(int? an)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue)
            {
                ViewBag.Mesaj = "Nu ai un apartament asignat. Contacteaza administratorul.";
                return View(new List<PublicareCheltuieliLunare>());
            }

            var ap = await _context.Apartamente
                .Include(a => a.Bloc)
                .FirstOrDefaultAsync(a => a.Id == user.ApartamentId.Value);

            if (ap == null)
            {
                ViewBag.Mesaj = "Apartamentul asignat nu exista.";
                return View(new List<PublicareCheltuieliLunare>());
            }

            ViewBag.Bloc = ap.Bloc;

            var scaraLocatar = (ap.Scara ?? "").Trim();

            var aniDisponibili = await _context.PublicariCheltuieliLunare
                .Where(p => p.BlocId == ap.BlocId)
                .Where(p =>
                    string.IsNullOrWhiteSpace(p.Scara) ||
                    (!string.IsNullOrWhiteSpace(scaraLocatar) && p.Scara == scaraLocatar)
                )
                .Select(p => p.An)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            ViewBag.AniDisponibili = aniDisponibili;
            ViewBag.AnSelectat = an;

            // lista (cu filtru an)
            var q = _context.PublicariCheltuieliLunare
                .Where(p => p.BlocId == ap.BlocId)
                .Where(p =>
                    string.IsNullOrWhiteSpace(p.Scara) ||
                    (!string.IsNullOrWhiteSpace(scaraLocatar) && p.Scara == scaraLocatar)
                );

            if (an.HasValue)
                q = q.Where(p => p.An == an.Value);

            var lista = await q
                .OrderByDescending(p => p.An)
                .ThenByDescending(p => p.Luna)
                .ThenByDescending(p => p.DataPublicare)
                .ToListAsync();

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> DescarcaPublicare(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.ApartamentId.HasValue) return Forbid();

            var ap = await _context.Apartamente
                .FirstOrDefaultAsync(a => a.Id == user.ApartamentId.Value);

            if (ap == null) return Forbid();

            var pub = await _context.PublicariCheltuieliLunare
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pub == null) return NotFound();

            if (pub.BlocId != ap.BlocId) return Forbid();

            var scaraLocatar = (ap.Scara ?? "").Trim();
            var scaraPub = (pub.Scara ?? "").Trim();

            bool ePentruToate = string.IsNullOrWhiteSpace(scaraPub);
            bool ePentruScaraLui = !string.IsNullOrWhiteSpace(scaraLocatar) && scaraPub == scaraLocatar;

            if (!ePentruToate && !ePentruScaraLui)
                return Forbid();

            if (string.IsNullOrWhiteSpace(pub.FileUrl))
                return NotFound("Fisierul nu este disponibil.");

            var rel = pub.FileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_env.WebRootPath, rel);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Fisierul nu exista pe server.");

            var fileName = Path.GetFileName(fullPath);

            return PhysicalFile(
                fullPath,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
