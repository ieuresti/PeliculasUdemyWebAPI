using PeliculasUdemyWebAPI.Modelos;

namespace PeliculasUdemyWebAPI.Repositorio.IRepositorio
{
    public interface IPeliculaRepository
    {
        Task<ICollection<Pelicula>> GetPeliculasAsync();
        Task<ICollection<Pelicula>> GetPeliculasEnCategoriaAsync(int categoriaId);
        Task<IEnumerable<Pelicula>> BuscarPeliculaAsync(string nombre);
        Task<Pelicula?> GetPeliculaAsync(int peliculaId);
        Task<bool> ExistePeliculaAsync(int peliculaId);
        Task<bool> ExistePeliculaAsync(string titulo);
        Task<bool> CrearPeliculaAsync(Pelicula pelicula);
        Task<bool> ActualizarPeliculaAsync(Pelicula pelicula);
        Task<bool> BorrarPeliculaAsync(Pelicula pelicula);
        Task<bool> GuardarAsync();
    }
}
