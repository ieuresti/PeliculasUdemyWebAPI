using Microsoft.EntityFrameworkCore;
using PeliculasUdemyWebAPI.Data;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;

namespace PeliculasUdemyWebAPI.Repositorio
{
    public class PeliculaRepository : IPeliculaRepository
    {
        private readonly ApplicationDbContext context;

        public PeliculaRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> ActualizarPeliculaAsync(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            context.Peliculas.Update(pelicula);
            return await GuardarAsync();
        }

        public async Task<bool> BorrarPeliculaAsync(Pelicula pelicula)
        {
            context.Peliculas.Remove(pelicula);
            return await GuardarAsync();
        }

        public async Task<IEnumerable<Pelicula>> BuscarPeliculaAsync(string nombre)
        {
            IQueryable<Pelicula> query = context.Peliculas;
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(
                    p => p.Nombre.ToLower().Contains(nombre.ToLower()) || 
                    p.Descripcion.ToLower().Contains(nombre.ToLower()));
            }
            return await query.ToListAsync();
        }

        public async Task<bool> CrearPeliculaAsync(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            context.Peliculas.Add(pelicula);
            return await GuardarAsync();
        }

        public async Task<bool> ExistePeliculaAsync(int peliculaId)
        {
            return await context.Peliculas.AnyAsync(p => p.Id == peliculaId);
        }

        public async Task<bool> ExistePeliculaAsync(string titulo)
        {
            return await context.Peliculas.AnyAsync(p => p.Nombre.ToLower().Trim() == titulo.ToLower().Trim());
        }

        public async Task<Pelicula?> GetPeliculaAsync(int peliculaId)
        {
            return await context.Peliculas.FirstOrDefaultAsync(p => p.Id == peliculaId);
        }

        public async Task<ICollection<Pelicula>> GetPeliculasAsync()
        {
            return await context.Peliculas.OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<ICollection<Pelicula>> GetPeliculasEnCategoriaAsync(int categoriaId)
        {
            return await context.Peliculas
                .Where(p => p.CategoriaId == categoriaId)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<bool> GuardarAsync()
        {
            return await context.SaveChangesAsync() > 0; // Devuelve true si se guardaron los cambios, false en caso contrario
        }
    }
}
