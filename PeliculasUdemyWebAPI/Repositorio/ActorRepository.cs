using Microsoft.EntityFrameworkCore;
using PeliculasUdemyWebAPI.Data;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;

namespace PeliculasUdemyWebAPI.Repositorio
{
    public class ActorRepository : IActorRepository
    {
        private readonly ApplicationDbContext context;

        public ActorRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> ActualizarActorAsync(Actor actor)
        {
            context.Actores.Update(actor);
            return await GuardarAsync();
        }

        public async Task<bool> BorrarActorAsync(Actor actor)
        {
            context.Actores.Remove(actor);
            return await GuardarAsync();
        }

        public async Task<bool> CrearActorAsync(Actor actor)
        {
            context.Actores.Add(actor);
            return await GuardarAsync();
        }

        public async Task<bool> ExisteActorAsync(int actorId)
        {
            return await context.Actores.AnyAsync(a => a.Id == actorId);
        }

        public async Task<bool> ExisteActorAsync(string nombre)
        {
            return await context.Actores.AnyAsync(a => a.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
        }

        public async Task<Actor?> GetActorAsync(int actorId)
        {
            return await context.Actores.FirstOrDefaultAsync(a => a.Id == actorId);
        }

        public async Task<ICollection<Actor>> GetActoresAsync(int pageNumber, int pageSize)
        {
            return await context.Actores
                .OrderBy(a => a.Nombre)
                .Skip((pageNumber - 1) * pageSize) // Calcula el número de registros a omitir según la página actual y el tamaño de página
                .Take(pageSize) // Toma el número de registros según el tamaño de página
                .ToListAsync();
        }

        public int GetTotalActores()
        {
            return context.Actores.Count();
        }

        public async Task<bool> GuardarAsync()
        {
            return await context.SaveChangesAsync() > 0; // Devuelve true si se guardaron los cambios, false en caso contrario
        }
    }
}
