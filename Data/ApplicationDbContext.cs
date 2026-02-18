using AdministrareBlocMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdministrareBlocMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<Locatar>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Bloc> Blocuri { get; set; }
        public DbSet<Apartament> Apartamente { get; set; }
        public DbSet<Anunt> Anunturi { get; set; }
        public DbSet<Intretinere> Intretineri { get; set; }
        public DbSet<IntretinereDetaliu> IntretineriDetalii { get; set; }
        public DbSet<AdministrareBlocMVC.Models.PublicareCheltuieliLunare> PublicariCheltuieliLunare { get; set; }
        public DbSet<Chirias> Chiriasi { get; set; }




    }
}