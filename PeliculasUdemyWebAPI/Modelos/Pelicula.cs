using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeliculasUdemyWebAPI.Modelos
{
    public class Pelicula
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(70, ErrorMessage = "El campo {0} debe contener {1} caracteres o menos")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(300, ErrorMessage = "El campo {0} debe contener {1} caracteres o menos")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Range(1, 300, ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        public int Duracion { get; set; }

        [Unicode(false)] // Va a ser una URL y no se necesitan todos los caracteres Unicode (emojis, acentos, etc.)
        public string? Poster { get; set; }

        public enum TipoClasificacion
        {
            G,
            PG,
            PG13,
            R,
            NC17
        }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoClasificacion Clasificacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime FechaCreacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int CategoriaId { get; set; } // Clave foránea para la relación con la entidad Categoria
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; } // Propiedad de navegación para la relación con la entidad Categoria
    }
}
