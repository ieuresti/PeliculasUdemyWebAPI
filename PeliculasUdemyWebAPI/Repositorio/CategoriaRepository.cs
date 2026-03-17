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

        public bool ActualizarCategoria(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            _context.Categorias.Update(categoria);
            return Guardar();
        }

        public bool BorrarCategoria(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);
            return Guardar();
        }

        public bool CrearCategoria(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            _context.Categorias.Add(categoria);
            return Guardar();
        }

        public bool ExisteCategoria(int categoriaId)
        {
            return _context.Categorias.Any(c => c.Id == categoriaId);
        }

        public bool ExisteCategoria(string nombre)
        {
            return _context.Categorias.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
        }

        public Categoria GetCategoria(int categoriaId)
        {
            return _context.Categorias.FirstOrDefault(c => c.Id == categoriaId);
        }

        public ICollection<Categoria> GetCategorias()
        {
            return _context.Categorias.OrderBy(c => c.Nombre).ToList();
        }

        public bool Guardar()
        {
            return _context.SaveChanges() >= 0 ? true : false; // Si se guardan los cambios correctamente, SaveChanges devuelve el número de objetos afectados, que será mayor o igual a 0. Si ocurre un error, devolverá un valor negativo.
        }
    }
}
