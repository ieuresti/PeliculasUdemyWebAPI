namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    public class PeliculaDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int Duracion { get; set; }
        public string? Poster { get; set; }

        public enum TipoClasificacion
        {
            G,
            PG,
            PG13,
            R,
            NC17
        }
        public TipoClasificacion Clasificacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Listas de categorías y actores asociados a la película para mostrar en la respuesta de la API
        public List<CategoriaDTO> Categorias { get; set; } = new List<CategoriaDTO>();
        public List<ActorDTO> Actores { get; set; } = new List<ActorDTO>();
    }
}
