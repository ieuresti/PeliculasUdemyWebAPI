using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeliculasUdemyWebAPI.Modelos;

namespace PeliculasUdemyWebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // IdentityDbContext se utiliza para manejar la autenticacion y autorizacion de usuarios, roles, etc.
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // OnModelCreating se utiliza para configurar el modelo de datos, como las relaciones entre entidades, claves primarias, etc.
            base.OnModelCreating(modelBuilder); // Si esto no esta, no se aplicaran las configuraciones de IdentityDbContext
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Actor> Actores { get; set; }
    }
}
