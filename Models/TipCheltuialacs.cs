using System.ComponentModel.DataAnnotations;

namespace AdministrareBlocMVC.Models
{
    public enum TipCheltuiala
    {
        Apa_Rece = 1,

        Apa_Calda = 2,

        Gunoi = 3,

        Curatenie = 4,

        Lift = 5,

        Iluminat_Spatii_Comune = 6,

        [Display(Name = "Fond Reparatii")]
        Fond_Reparatii = 7,

        Fond_Rulment = 8,

        [Display(Name = "Alte Cheltuieli")]
        Alte_Cheltuieli = 99
    }
}
