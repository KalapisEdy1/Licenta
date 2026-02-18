using AdministrareBlocMVC.Models;

namespace AdministrareBlocMVC.ViewModels
{
    public class ApartamentulMeuViewModel
    {
        public string? Email { get; set; }

       
        public int NrPersoane { get; set; }

        public int? ApartamentId { get; set; }

        public int? NumarApartament { get; set; }
        public int? Etaj { get; set; }
        public string? NumeBloc { get; set; }

        public string? Scara { get; set; }

        public string? NumeProprietar { get; set; }

        public int NumarCamere { get; set; }
        public decimal SuprafataMp { get; set; }
        public string? TelefonProprietar { get; set; }
        public string? NumeChirias { get; set; }
        public string? TelefonChirias { get; set; }
    }
}
