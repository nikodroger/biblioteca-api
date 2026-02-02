namespace BibliotecaAPI.DTOs
{
    public class RespuestaAutentificacionDTO
    {
        public required string Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}
