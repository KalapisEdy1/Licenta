using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdministrareBlocMVC.Data;
using AdministrareBlocMVC.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace AdministrareBlocMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class IntretinereController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IntretinereController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

       
        public async Task<IActionResult> Index()
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .ToListAsync();

            return View(blocuri);
        }

      
        public async Task<IActionResult> ListaPeBloc(int id)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == id);
            if (bloc == null) return NotFound();

            ViewBag.Bloc = bloc;

            var apartamente = await _context.Apartamente
                .Where(a => a.BlocId == id)
                .OrderBy(a => a.Numar)
                .ToListAsync();

            var apartamenteIds = apartamente.Select(a => a.Id).ToList();

            var intretineri = await _context.Intretineri
                .Where(i => apartamenteIds.Contains(i.ApartamentId))
                .OrderByDescending(i => i.An)
                .ThenByDescending(i => i.Luna)
                .Take(10)
                .ToListAsync();

            ViewBag.Intretineri = intretineri;

            return View(apartamente);
        }

        public async Task<IActionResult> Create(int apartamentId)
        {
            var ap = await _context.Apartamente
                .Include(a => a.Bloc)
                .FirstOrDefaultAsync(a => a.Id == apartamentId);

            if (ap == null) return NotFound();

            ViewBag.Apartament = ap;

            var model = new Intretinere
            {
                ApartamentId = apartamentId,
                Luna = DateTime.Now.Month,
                An = DateTime.Now.Year
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApartamentId,Luna,An")] Intretinere model)
        {
            bool exista = await _context.Intretineri.AnyAsync(i =>
                i.ApartamentId == model.ApartamentId &&
                i.Luna == model.Luna &&
                i.An == model.An);

            if (exista)
                ModelState.AddModelError("", "Exista deja intretinere pentru acest apartament in luna/an selectate.");

            if (!ModelState.IsValid)
            {
                var ap2 = await _context.Apartamente
                    .Include(a => a.Bloc)
                    .FirstOrDefaultAsync(a => a.Id == model.ApartamentId);

                ViewBag.Apartament = ap2;
                return View(model);
            }

            var noua = new Intretinere
            {
                ApartamentId = model.ApartamentId,
                Luna = model.Luna,
                An = model.An,
                DataGenerare = DateTime.Now,
                Total = 0,
                StatusPlata = StatusPlata.Neplatita
            };

            _context.Intretineri.Add(noua);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detalii), new { id = noua.Id });
        }

     
        public async Task<IActionResult> Detalii(int id)
        {
            var intretinere = await _context.Intretineri
                .Include(i => i.Apartament)
                    .ThenInclude(a => a.Bloc)
                .Include(i => i.Detalii)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intretinere == null) return NotFound();

            return View(intretinere);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcheazaPlatita(int id)
        {
            var intr = await _context.Intretineri.FirstOrDefaultAsync(i => i.Id == id);
            if (intr == null) return NotFound();

            intr.StatusPlata = StatusPlata.Platita;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detalii), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaDetaliu(int intretinereId, TipCheltuiala tipCheltuiala, decimal suma)
        {
            var intr = await _context.Intretineri.FirstOrDefaultAsync(i => i.Id == intretinereId);
            if (intr == null) return NotFound();

            if (intr.StatusPlata == StatusPlata.Platita)
            {
                TempData["Err"] = "Nu poti modifica detaliile. Intretinerea este marcata ca PLATITA.";
                return RedirectToAction(nameof(Detalii), new { id = intretinereId });
            }

            var det = new IntretinereDetaliu
            {
                IntretinereId = intretinereId,
                TipCheltuiala = tipCheltuiala,
                Suma = suma
            };

            _context.IntretineriDetalii.Add(det);
            await _context.SaveChangesAsync();

            await RecalculeazaTotal(intretinereId);

            return RedirectToAction(nameof(Detalii), new { id = intretinereId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeDetaliu(int id)
        {
            var det = await _context.IntretineriDetalii.FirstOrDefaultAsync(d => d.Id == id);
            if (det == null) return NotFound();

            var intr = await _context.Intretineri.FirstOrDefaultAsync(i => i.Id == det.IntretinereId);
            if (intr == null) return NotFound();

            if (intr.StatusPlata == StatusPlata.Platita)
            {
                TempData["Err"] = "Nu poti sterge cheltuieli. Intretinerea este marcata ca PLATITA.";
                return RedirectToAction(nameof(Detalii), new { id = intr.Id });
            }

            int intretinereId = det.IntretinereId;

            _context.IntretineriDetalii.Remove(det);
            await _context.SaveChangesAsync();

            await RecalculeazaTotal(intretinereId);

            return RedirectToAction(nameof(Detalii), new { id = intretinereId });
        }

        private async Task RecalculeazaTotal(int intretinereId)
        {
            var total = await _context.IntretineriDetalii
                .Where(d => d.IntretinereId == intretinereId)
                .SumAsync(d => (decimal?)d.Suma) ?? 0;

            var intr = await _context.Intretineri.FirstOrDefaultAsync(i => i.Id == intretinereId);
            if (intr != null)
            {
                intr.Total = total;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> IstoricApartament(int apartamentId, int? an)
        {
            var ap = await _context.Apartamente
                .Include(a => a.Bloc)
                .FirstOrDefaultAsync(a => a.Id == apartamentId);

            if (ap == null) return NotFound();

            var q = _context.Intretineri.Where(i => i.ApartamentId == apartamentId);

            if (an.HasValue)
                q = q.Where(i => i.An == an.Value);

            var lista = await q
                .OrderByDescending(i => i.An)
                .ThenByDescending(i => i.Luna)
                .ToListAsync();

            var aniDisponibili = await _context.Intretineri
                .Where(i => i.ApartamentId == apartamentId)
                .Select(i => i.An)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            ViewBag.Apartament = ap;
            ViewBag.AniDisponibili = aniDisponibili;
            ViewBag.AnSelectat = an;

            return View("IstoricIntretinereApartament", lista);
        }

        [HttpGet]
        public async Task<IActionResult> Statistica()
        {
            var blocuri = await _context.Blocuri
                .OrderBy(b => b.Nume)
                .ToListAsync();

            ViewBag.Blocuri = blocuri;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> StatisticaLuna(int apartamentId, int luna, int an)
        {
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
                .Select(g => new { Label = g.Key.ToString(), Total = g.Sum(x => x.Suma) })
                .OrderByDescending(x => x.Total)
                .ToList();

            return Json(new
            {
                labels = data.Select(x => x.Label),
                values = data.Select(x => x.Total)
            });
        }

        [HttpGet]
        public async Task<IActionResult> Export(int blocId)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) return NotFound();

            var apartamente = await _context.Apartamente
                .Where(a => a.BlocId == blocId)
                .OrderBy(a => a.Numar)
                .ToListAsync();

            ViewBag.Bloc = bloc;
            ViewBag.Apartamente = apartamente;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(int blocId, int apartamentId, int an)
        {
            if (an < 2010)
            {
                TempData["Err"] = "An invalid. Trebuie sa fie >= 2010.";
                return RedirectToAction(nameof(Export), new { blocId });
            }

            return RedirectToAction(nameof(ExportExcelAn), new { apartamentId, an });
        }

        public async Task<IActionResult> ExportExcelAn(int apartamentId, int an)
        {
            return NotFound("Metoda ExportExcelAn exista deja la tine. Pastreaz-o pe a ta.");
        }

       

        [HttpGet]
        public async Task<IActionResult> ExportCheltuieliApLuna(int blocId)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) return NotFound();

            ViewBag.Bloc = bloc;
            ViewBag.Scari = ParseScari(bloc.Scari);

            ViewBag.Luna = DateTime.Now.Month;
            ViewBag.An = DateTime.Now.Year;

            ViewBag.DataColectare = DateTime.Today.AddHours(9).ToString("yyyy-MM-ddTHH:mm");
            ViewBag.OraSfarsit = "11:00";

            return View("ExportCheltuieliApLuna");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportCheltuieliApLuna(int blocId, int luna, int an, string? scara, string dataColectare, string oraSfarsit)
        {
            if (luna < 1 || luna > 12)
            {
                TempData["Err"] = "Luna invalida (1..12).";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            if (an < 2010 || an > 2100)
            {
                TempData["Err"] = "An invalid (ex: 2010..2100).";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            if (string.IsNullOrWhiteSpace(dataColectare))
            {
                TempData["Err"] = "Completeaza data + ora colectarii.";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            if (string.IsNullOrWhiteSpace(oraSfarsit))
            {
                TempData["Err"] = "Completeaza ora de sfarsit a colectarii.";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            return RedirectToAction(nameof(ExportCheltuieliApLunaExcel), new { blocId, luna, an, scara, dataColectare, oraSfarsit });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublicaCheltuieliApLuna(int blocId, int luna, int an, string? scara, string dataColectare, string oraSfarsit)
        {
            if (luna < 1 || luna > 12)
            {
                TempData["Err"] = "Luna invalida (1..12).";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            if (an < 2010 || an > 2100)
            {
                TempData["Err"] = "An invalid (ex: 2010..2100).";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            if (!DateTime.TryParse(dataColectare, out var dtColectare))
            {
                TempData["Err"] = "Data colectarii este invalida.";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            if (string.IsNullOrWhiteSpace(oraSfarsit))
            {
                TempData["Err"] = "Completeaza ora de sfarsit a colectarii.";
                return RedirectToAction(nameof(ExportCheltuieliApLuna), new { blocId });
            }

            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) return NotFound();

            var scariBloc = ParseScari(bloc.Scari);
            if (scariBloc.Length > 0 && !string.IsNullOrWhiteSpace(scara))
            {
                var scTrim = scara.Trim();
                bool exista = scariBloc.Any(s => string.Equals(s, scTrim, StringComparison.OrdinalIgnoreCase));
                if (!exista) scara = null;
                else scara = scTrim;
            }
            else
            {
                scara = null;
            }

            var fileBytes = await BuildCheltuieliExcelBytes(blocId, luna, an, scara, dtColectare, oraSfarsit.Trim());

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "intretinere");
            Directory.CreateDirectory(uploadsDir);

            var scaraPart = string.IsNullOrWhiteSpace(scara) ? "TOATE" : scara.Trim();
            var fileName = $"Cheltuieli_Bloc{blocId}_{scaraPart}_{luna:D2}-{an}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var fullPath = Path.Combine(uploadsDir, fileName);
            await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

            var fileUrl = $"/uploads/intretinere/{fileName}";

            var pub = new PublicareCheltuieliLunare
            {
                BlocId = blocId,
                Scara = string.IsNullOrWhiteSpace(scara) ? null : scara.Trim(),
                Luna = luna,
                An = an,
                DataPublicare = DateTime.Now,
                DataColectare = dtColectare,
                FileUrl = fileUrl
            };

            _context.PublicariCheltuieliLunare.Add(pub);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Publicarea a fost salvata cu succes.";
            return RedirectToAction(nameof(PublicariBloc), new { blocId });
        }

        [HttpGet]
        public async Task<IActionResult> PublicariBloc(int blocId, int? an)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) return NotFound();

            ViewBag.Bloc = bloc;

            var aniDisponibili = await _context.PublicariCheltuieliLunare
                .Where(p => p.BlocId == blocId)
                .Select(p => p.An)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            ViewBag.AniDisponibili = aniDisponibili;
            ViewBag.AnSelectat = an;

            var q = _context.PublicariCheltuieliLunare
                .Where(p => p.BlocId == blocId);

            if (an.HasValue)
                q = q.Where(p => p.An == an.Value);

            var list = await q
                .OrderByDescending(p => p.An)
                .ThenByDescending(p => p.Luna)
                .ThenByDescending(p => p.DataPublicare)
                .ToListAsync();

            return View("PublicariBloc", list);
        }

        [HttpGet]
        public async Task<IActionResult> DescarcaPublicare(int id)
        {
            var pub = await _context.PublicariCheltuieliLunare
                .Include(p => p.Bloc)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pub == null) return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath, pub.FileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(fullPath)) return NotFound("Fisierul nu exista pe server.");

            var downloadName = $"Cheltuieli_{pub.Bloc?.Nume ?? "Bloc"}_{pub.Luna:D2}-{pub.An}.xlsx";
            return PhysicalFile(fullPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", downloadName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCheltuieliApLunaExcel(int blocId, int luna, int an, string? scara, string dataColectare, string oraSfarsit)
        {
            if (!DateTime.TryParse(dataColectare, out var dtColectare))
                dtColectare = DateTime.Now;

            if (string.IsNullOrWhiteSpace(oraSfarsit))
                oraSfarsit = "11:00";

            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) return NotFound();

            var scariBloc = ParseScari(bloc.Scari);
            if (scariBloc.Length > 0 && !string.IsNullOrWhiteSpace(scara))
            {
                var scTrim = scara.Trim();
                bool exista = scariBloc.Any(s => string.Equals(s, scTrim, StringComparison.OrdinalIgnoreCase));
                if (!exista) scara = null;
                else scara = scTrim;
            }
            else
            {
                scara = null;
            }

            var bytes = await BuildCheltuieliExcelBytes(blocId, luna, an, scara, dtColectare, oraSfarsit.Trim());

            var blocName = bloc?.Nume ?? $"Bloc{blocId}";
            var scaraPart = string.IsNullOrWhiteSpace(scara) ? "" : $"_{scara.Trim()}";

            var fileName = $"Cheltuieli_{blocName}{scaraPart}_{luna:D2}-{an}.xlsx";

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<byte[]> BuildCheltuieliExcelBytes(int blocId, int luna, int an, string? scara, DateTime dataColectare, string oraSfarsit)
        {
            var bloc = await _context.Blocuri.FirstOrDefaultAsync(b => b.Id == blocId);
            if (bloc == null) throw new Exception("Bloc invalid");

            var scariBloc = ParseScari(bloc.Scari);
            bool blocAreScari = scariBloc.Length > 0;

            scara = string.IsNullOrWhiteSpace(scara) ? null : scara.Trim();

            if (blocAreScari && scara != null)
            {
                bool exista = scariBloc.Any(s => string.Equals(s, scara, StringComparison.OrdinalIgnoreCase));
                if (!exista) scara = null;
            }
            else
            {
                scara = null;
            }

            var apQuery = _context.Apartamente.Where(a => a.BlocId == blocId);

            if (blocAreScari && scara != null)
            {
                var scLower = scara.ToLower();
                apQuery = apQuery.Where(a => a.Scara != null && a.Scara.ToLower() == scLower);
            }

            var apartamente = await apQuery.OrderBy(a => a.Numar).ToListAsync();
            var apartamenteIds = apartamente.Select(a => a.Id).ToList();

            var intretineri = await _context.Intretineri
                .Include(i => i.Detalii)
                .Where(i => apartamenteIds.Contains(i.ApartamentId) && i.Luna == luna && i.An == an)
                .ToListAsync();

            var tipuri = Enum.GetValues(typeof(TipCheltuiala))
                .Cast<TipCheltuiala>()
                .OrderBy(t => (int)t)
                .ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("CHELTUIELI");

            ws.Cell(1, 1).Value = "Bloc:";
            ws.Cell(1, 2).Value = bloc.Nume;

            ws.Cell(2, 1).Value = "Adresa:";
            ws.Cell(2, 2).Value = bloc.Adresa;

            ws.Cell(3, 1).Value = "Luna/An:";
            ws.Cell(3, 2).Value = $"{luna:D2}/{an}";

            int infoRow = 4;

            if (blocAreScari)
            {
                ws.Cell(infoRow, 1).Value = "Scara:";
                ws.Cell(infoRow, 2).Value = scara ?? "TOATE";
                infoRow++;
            }

            ws.Cell(infoRow, 1).Value = "Data colectare:";
            ws.Cell(infoRow, 2).Value = $"{dataColectare:dd.MM.yyyy HH:mm}-{oraSfarsit}";
            infoRow++;

            ws.Cell(infoRow, 1).Value = "Nota:";
            ws.Cell(infoRow, 2).Value = "Incasarile se pot faci si online la acest IBAN : RO78 RZBR 0000 0600 2228 5224.";
            infoRow++;

            int startRow = infoRow + 1;
            int totalCol = 2 + tipuri.Count + 1;

            int WriteHeader(int r)
            {
                int c = 1;
                ws.Cell(r, c++).Value = "Ap";
                ws.Cell(r, c++).Value = "Etaj";

                foreach (var t in tipuri)
                    ws.Cell(r, c++).Value = t.ToString();

                ws.Cell(r, c).Value = "Total";

                ws.Range(r, 1, r, totalCol).Style.Font.Bold = true;
                ws.Range(r, 1, r, totalCol).Style.Fill.BackgroundColor = XLColor.LightGray;

                return r + 1;
            }

            int row = startRow;

            var totaluriTipGeneral = tipuri.ToDictionary(t => t, t => 0m);
            decimal totalGeneral = 0m;

            if (blocAreScari)
            {
                var grupuri = apartamente
                    .GroupBy(a => string.IsNullOrWhiteSpace(a.Scara) ? "FARA SCARA" : a.Scara.Trim())
                    .OrderBy(g => g.Key);

                foreach (var g in grupuri)
                {
                    ws.Cell(row, 1).Value = $"SCARA {g.Key}";
                    ws.Range(row, 1, row, totalCol).Merge();
                    ws.Range(row, 1, row, totalCol).Style.Font.Bold = true;
                    ws.Range(row, 1, row, totalCol).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                    row += 2;

                    row = WriteHeader(row);

                    var totaluriTipScara = tipuri.ToDictionary(t => t, t => 0m);
                    decimal totalScara = 0m;

                    foreach (var ap in g.OrderBy(a => a.Numar))
                    {
                        var intr = intretineri.FirstOrDefault(i => i.ApartamentId == ap.Id);

                        int c = 1;
                        ws.Cell(row, c++).Value = ap.Numar;
                        ws.Cell(row, c++).Value = ap.Etaj;

                        decimal totalAp = 0m;

                        foreach (var t in tipuri)
                        {
                            decimal sumaTip = 0m;
                            if (intr?.Detalii != null)
                                sumaTip = intr.Detalii.Where(d => d.TipCheltuiala == t).Sum(d => d.Suma);

                            ws.Cell(row, c++).Value = sumaTip;

                            totalAp += sumaTip;
                            totaluriTipScara[t] += sumaTip;
                            totaluriTipGeneral[t] += sumaTip;
                        }

                        ws.Cell(row, totalCol).Value = $"{totalAp} RON";

                        totalScara += totalAp;
                        totalGeneral += totalAp;

                        row++;
                    }

                    row += 2;
                }

            }
            else
            {
                row = WriteHeader(row);

                foreach (var ap in apartamente)
                {
                    var intr = intretineri.FirstOrDefault(i => i.ApartamentId == ap.Id);

                    int c = 1;
                    ws.Cell(row, c++).Value = ap.Numar;
                    ws.Cell(row, c++).Value = ap.Etaj;

                    decimal totalAp = 0m;

                    foreach (var t in tipuri)
                    {
                        decimal sumaTip = 0m;
                        if (intr?.Detalii != null)
                            sumaTip = intr.Detalii.Where(d => d.TipCheltuiala == t).Sum(d => d.Suma);

                        ws.Cell(row, c++).Value = sumaTip;

                        totalAp += sumaTip;
                        totaluriTipGeneral[t] += sumaTip;
                    }

                    ws.Cell(row, totalCol).Value = $"{totalAp} RON";
                    totalGeneral += totalAp;

                    row++;
                }

            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
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
