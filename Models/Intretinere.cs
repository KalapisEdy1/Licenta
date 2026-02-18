using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public enum StatusPlata
    {
        Neplatita = 0,
        Platita = 1
    }
    public class Intretinere
    {
        public int Id { get; set; }

        [Required]
        public int ApartamentId { get; set; }
        public Apartament? Apartament { get; set; }

        [Range(1, 12)]
        public int Luna { get; set; }

        [Range(2000, 2100)]
        public int An { get; set; }

        public DateTime DataGenerare { get; set; } = DateTime.Now;

        public decimal Total { get; set; } = 0;

        [Display(Name = "Status plata")]
        public StatusPlata StatusPlata { get; set; } = StatusPlata.Neplatita;

        public List<IntretinereDetaliu> Detalii { get; set; } = new();
    }
}
