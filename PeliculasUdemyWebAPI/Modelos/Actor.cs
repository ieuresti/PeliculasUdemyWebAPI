using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PeliculasUdemyWebAPI.Modelos
{
    public class Actor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(70, ErrorMessage = "El campo {0} debe contener {1} caracteres o menos")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Unicode(false)] // Va a ser una URL y no se necesitan todos los caracteres Unicode (emojis, acentos, etc.)
        public string? Foto { get; set; }
    }
}
