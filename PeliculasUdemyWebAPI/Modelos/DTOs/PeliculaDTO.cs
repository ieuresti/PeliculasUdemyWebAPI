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

        public int CategoriaId { get; set; }
    }
}
