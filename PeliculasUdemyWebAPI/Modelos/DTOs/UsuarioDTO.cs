namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    // Este DTO se utiliza para enviar al cliente la información del usuario autenticado, en este caso solo el email, pero se podrían agregar otros campos como el nombre, roles, etc.
    public class UsuarioDTO
    {
        public string Email { get; set; }
    }
}
