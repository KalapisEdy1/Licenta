using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public class PublicareCheltuieliLunare
    {
        public int Id { get; set; }

        [Required]
        public int BlocId { get; set; }
        public Bloc? Bloc { get; set; }

        [MaxLength(20)]
        public string? Scara { get; set; }

        [Range(1, 12)]
        public int Luna { get; set; }

        [Range(2010, 2100)]
        public int An { get; set; }

        public DateTime DataPublicare { get; set; }

        public DateTime DataColectare { get; set; }

        [Required]
        public string FileUrl { get; set; } = "";
    }
}
