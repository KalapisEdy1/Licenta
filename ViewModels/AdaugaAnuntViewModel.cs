using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.ViewModels
{
    public class AdaugaAnuntViewModel
    {
        [Required]
        [Display(Name = "Bloc")]
        public int BlocId { get; set; }

        public string? Scara { get; set; }

        [Required]
        [Display(Name = "Anunt")]
        [MaxLength(2000)]
        public string Mesaj { get; set; }
    }
}
