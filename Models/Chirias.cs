using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public class Chirias
    {
        public int Id { get; set; }

        [Required]
        public int ApartamentId { get; set; }

        [Display(Name = "Nume Chirias")]
        [MaxLength(100)]
        public string? NumeChirias { get; set; }

        [Display(Name = "Telefon Chirias")]
        [MaxLength(30)]
        public string? TelefonChirias { get; set; }

        public Apartament? Apartament { get; set; }
    }
}
