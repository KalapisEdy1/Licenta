using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.ViewModels
{
    public class ApartamentCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 50)]
        public int Numar { get; set; }

        [Required]
        [Range(0, 50)]
        public int Etaj { get; set; }

        [Required]
        [Display(Name = "Nume Bloc")]
        public int BlocId { get; set; }

        [MaxLength(20)]
        public string? Scara { get; set; }

        [Display(Name = "Nume Proprietar")]
        [MaxLength(100)]
        public string? NumeProprietar { get; set; }

        [Display(Name = "Numar Camere")]
        [Range(0, 20)]
        public int NumarCamere { get; set; }

        [Display(Name = "Suprafata(Mp)")]
        [Range(0, 10000)]
        public decimal SuprafataMp { get; set; }

        [Display(Name = "Telefon Proprietar")]
        [MaxLength(30)]
        public string? TelefonProprietar { get; set; }

        [Display(Name = "Numar Persoane")]
        [Range(0, 50)]
        public int NrPersoane { get; set; } = 0;

        public bool AdaugaChirias { get; set; }

        [Display(Name = "Nume Chirias")]
        [MaxLength(100)]
        public string? NumeChirias { get; set; }

        [Display(Name = "Telefon Chirias")]
        [MaxLength(30)]
        public string? TelefonChirias { get; set; }

        public bool StergeChirias { get; set; }
        public bool StergeChiriasConfirmat { get; set; }
        public bool AreChirias { get; set; }
    }
}
