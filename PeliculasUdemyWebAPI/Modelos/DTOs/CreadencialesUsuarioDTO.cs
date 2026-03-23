using System.ComponentModel.DataAnnotations;

namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    // Este DTO se utiliza para recibir las credenciales del usuario (email y contraseña) en el endpoint de autenticacion (login) y en el endpoint de registro (register)
    public class CreadencialesUsuarioDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "El campo {0} no es un correo electrónico válido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos 6 caracteres.")]
        public required string Password { get; set; }
    }
}
