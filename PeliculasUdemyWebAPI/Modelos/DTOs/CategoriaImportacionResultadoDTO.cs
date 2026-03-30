namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    // DTO para representar el resultado de la importación de categorías desde un archivo Excel
    public class CategoriaImportacionResultadoDTO
    {
        public int TotalFilas { get; set; }
        public int CategoriasCreadas { get; set; }
        public int CategoriasConErrores { get; set; }
        public List<ErrorImportacionDTO> Errores { get; set; } = new(); // Inicializamos la lista de errores para evitar null reference exceptions
    }

    public class ErrorImportacionDTO
    {
        public int Fila { get; set; }
        public string Campo { get; set; }
        public string Mensaje { get; set; }
    }
}
