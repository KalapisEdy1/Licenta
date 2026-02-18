using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public class Anunt
    {
        public int Id { get; set; }

        [Required]
        public int BlocId { get; set; }
        public Bloc? Bloc { get; set; }

        [MaxLength(20)]
        public string? Scara { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Mesaj { get; set; }

        public DateTime DataCreare { get; set; } = DateTime.Now;
    }
}
