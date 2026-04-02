namespace PeliculasUdemyWebAPI.Modelos
{
    public class PeliculaActor
    {
        public int PeliculaId { get; set; }
        public Pelicula Pelicula { get; set; } = null!; // Propiedad de navegación a la entidad Pelicula

        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!; // Propiedad de navegación a la entidad Actor
    }
}
