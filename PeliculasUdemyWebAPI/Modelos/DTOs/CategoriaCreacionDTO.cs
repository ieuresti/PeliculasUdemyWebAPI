using System.ComponentModel.DataAnnotations;

namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    public class CategoriaCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe contener {1} caracteres o menos")]
        public string Nombre { get; set; }
    }
}
