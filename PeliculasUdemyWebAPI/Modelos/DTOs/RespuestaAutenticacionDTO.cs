namespace PeliculasUdemyWebAPI.Modelos.DTOs
{
    // Este DTO se utiliza para enviar la respuesta de autenticacion al cliente, que incluye el token JWT generado y su fecha de expiracion
    public class RespuestaAutenticacionDTO
    {
        public required string Token { get; set; }

        public DateTime Expiracion { get; set; }
    }
}
