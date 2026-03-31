using PeliculasUdemyWebAPI.Modelos;

namespace PeliculasUdemyWebAPI.Repositorio.IRepositorio
{
    public interface IActorRepository
    {
        Task<ICollection<Actor>> GetActoresAsync(int pageNumber, int pageSize);
        int GetTotalActores();
        Task<Actor?> GetActorAsync(int actorId);
        Task<bool> ExisteActorAsync(int actorId);
        Task<bool> ExisteActorAsync(string nombre);
        Task<bool> CrearActorAsync(Actor actor);
        Task<bool> ActualizarActorAsync(Actor actor);
        Task<bool> BorrarActorAsync(Actor actor);
        Task<bool> GuardarAsync();
    }
}
