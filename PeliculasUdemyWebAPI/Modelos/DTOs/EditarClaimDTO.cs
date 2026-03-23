namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    // Este DTO se utiliza para recibir el email del usuario al que se le va a editar o eliminar un claim específico en el endpoint correspondiente del controlador de usuarios
    public class EditarClaimDTO
    {
        public required string Email { get; set; }
    }
}
