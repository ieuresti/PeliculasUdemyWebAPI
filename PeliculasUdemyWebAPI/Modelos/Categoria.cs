using System.ComponentModel.DataAnnotations;

namespace PeliculasUdemyWebAPI.Modelos
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe contener {1} caracteres o menos")]
        public string Nombre { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }
    }
}
