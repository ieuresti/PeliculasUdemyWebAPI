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

            // Configurar la relación muchos-a-muchos entre Pelicula y Categoria
            modelBuilder.Entity<PeliculaCategoria>()
                .HasKey(pc => new { pc.PeliculaId, pc.CategoriaId }); // Clave primaria compuesta de la tabla intermedia PeliculaCategoria

            modelBuilder.Entity<PeliculaCategoria>()
                .HasOne(pc => pc.Pelicula)
                .WithMany(p => p.PeliculaCategorias)
                .HasForeignKey(pc => pc.PeliculaId);

            modelBuilder.Entity<PeliculaCategoria>()
                .HasOne(pc => pc.Categoria)
                .WithMany(c => c.PeliculaCategorias)
                .HasForeignKey(pc => pc.CategoriaId);

            // Configurar la relación muchos-a-muchos entre Pelicula y Actor
            modelBuilder.Entity<PeliculaActor>()
                .HasKey(pa => new { pa.PeliculaId, pa.ActorId }); // Clave primaria compuesta de la tabla intermedia PeliculaActor

            modelBuilder.Entity<PeliculaActor>()
                .HasOne(pa => pa.Pelicula)
                .WithMany(p => p.PeliculaActores)
                .HasForeignKey(pa => pa.PeliculaId);

            modelBuilder.Entity<PeliculaActor>()
                .HasOne(pa => pa.Actor)
                .WithMany(a => a.PeliculaActores)
                .HasForeignKey(pa => pa.ActorId);
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<PeliculaCategoria> PeliculaCategorias { get; set; }
        public DbSet<PeliculaActor> PeliculaActores { get; set; }
    }
}
