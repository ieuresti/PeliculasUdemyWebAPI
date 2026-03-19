using Microsoft.EntityFrameworkCore;
using PeliculasUdemyWebAPI.Data;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;

namespace PeliculasUdemyWebAPI.Repositorio
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoriaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            _context.Categorias.Update(categoria);
            return await GuardarAsync();
        }

        public async Task<bool> BorrarCategoriaAsync(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);
            return await GuardarAsync();
        }

        public async Task<bool> CrearCategoriaAsync(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            _context.Categorias.Add(categoria);
            return await GuardarAsync();
        }

        public async Task<bool> ExisteCategoriaAsync(int categoriaId)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == categoriaId);
        }

        public async Task<bool> ExisteCategoriaAsync(string nombre)
        {
            return await _context.Categorias.AnyAsync(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
        }

        public async Task<Categoria?> GetCategoriaAsync(int categoriaId)
        {
            return await _context.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId);
        }

        public async Task<ICollection<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        }

        public async Task<bool> GuardarAsync()
        {
            return await _context.SaveChangesAsync() > 0; // Devuelve true si se guardaron los cambios, false en caso contrario
        }
    }
}
