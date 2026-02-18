using System.ComponentModel.DataAnnotations;
namespace AdministrareBlocMVC.ViewModels
{
    public class AsignareApartamentViewModel
    {
        [Required]
        public string UserId { get; set; }

        public string? Email { get; set; }

        [Display(Name = "Apartament")]
        public int? ApartamentId { get; set; }

        [Display(Name = "Bloc")]
        public int? BlocId { get; set; }

        [Display(Name = "Scara")]
        public string? Scara { get; set; }


    }
}
