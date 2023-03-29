using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedeSocial.BLL.Models;

namespace RedeSocial.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Midia> Midias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /*            modelBuilder.Entity<ApplicationUser>(e => {
                            e.ToTable(name: "ApplicationUser");
                            e.HasMany(p => p.PerfilId);
            //                e.WithMany(u => u.PerfilId);
                            //e.HasOne(p => p.Person).WithOne().HasForeignKey;
                        });
            */
            modelBuilder.Entity<ApplicationUser>()
                .ToTable(name: "ApplicationUser")
                .HasOne<Perfil>(b => b.Perfil);
//                .HasMany<ApplicationUser>();
            //.HasForeignKey;
        }
    }
}
