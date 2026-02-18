using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public class IntretinereDetaliu
    {
        public int Id { get; set; }

        [Required]
        public int IntretinereId { get; set; }
        public Intretinere? Intretinere { get; set; }

        [Required]
        public TipCheltuiala TipCheltuiala { get; set; }

        [Range(0, 999999)]
        public decimal Suma { get; set; }
    }
}
