namespace PeliculasUdemyWebAPI.Modelos
{
    public class PeliculaCategoria
    {
        public int PeliculaId { get; set; }
        public Pelicula Pelicula { get; set; } = null!; // Propiedad de navegación a la entidad Pelicula

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!; // Propiedad de navegación a la entidad Categoria
    }
}
