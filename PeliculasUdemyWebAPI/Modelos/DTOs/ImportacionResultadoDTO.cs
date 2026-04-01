namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    // DTO genérico para representar el resultado de cualquier importación desde un archivo Excel
    public class ImportacionResultadoDTO
    {
        public int TotalFilas { get; set; }
        public int RegistrosCreados { get; set; }
        public int RegistrosConErrores { get; set; }
        public List<ErrorImportacionDTO> Errores { get; set; } = new(); // Inicializamos la lista de errores para evitar null reference exceptions
    }

    public class ErrorImportacionDTO
    {
        public int Fila { get; set; }
        public string Campo { get; set; }
        public string Mensaje { get; set; }
    }
}
