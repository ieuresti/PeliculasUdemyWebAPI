using PeliculasUdemyWebAPI.Modelos;

namespace PeliculasUdemyWebAPI.Repositorio.IRepositorio
{
    public interface ICategoriaRepository
    {
        Task<ICollection<Categoria>> GetCategoriasAsync();
        Task<Categoria?> GetCategoriaAsync(int categoriaId);
        Task<bool> ExisteCategoriaAsync(int categoriaId);
        Task<bool> ExisteCategoriaAsync(string nombre);
        Task<bool> CrearCategoriaAsync(Categoria categoria);
        Task<bool> ActualizarCategoriaAsync(Categoria categoria);
        Task<bool> BorrarCategoriaAsync(Categoria categoria);
        Task<bool> GuardarAsync();
    }
}
