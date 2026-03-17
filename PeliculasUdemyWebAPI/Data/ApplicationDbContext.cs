using Microsoft.EntityFrameworkCore;
using PeliculasUdemyWebAPI.Modelos;

namespace PeliculasUdemyWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
    }
}
