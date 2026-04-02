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

        public List<PeliculaCategoria> PeliculaCategorias { get; set; } = new List<PeliculaCategoria>(); // Propiedad de navegacion a la tabla intermedia PeliculaCategoria
        public List<PeliculaActor> PeliculaActores { get; set; } = new List<PeliculaActor>(); // Propiedad de navegacion a la tabla intermedia PeliculaActor
    }
}
