using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace AdministrareBlocMVC.Models
{
    public class Locatar: IdentityUser
    {
        
        public int? ApartamentId { get; set; }

        public Apartament? Apartament { get; set; }
    }
}
