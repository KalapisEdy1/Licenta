using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public class Bloc
    {
        public int Id { get; set; }

        [Required]
        public string Nume { get; set; }

        public string Adresa { get; set; }

        [MaxLength(200)]
        [Display(Name = "Scari (separate prin virgula)")]
        [RegularExpression(@"^$|^[^,\s]+(,\s*[^,\s]+)*$", ErrorMessage = "Format invalid. Exemplu: A,B,C (fara virgula la final).")]
        public string? Scari { get; set; }

        public ICollection<Apartament> Apartamente { get; set; } = new List<Apartament>();
    }
}
