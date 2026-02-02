using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BibliotecaAPI.Servicios
{
    public class ServiciosUsuarios : IServiciosUsuarios
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IHttpContextAccessor contextAccessor;

        public ServiciosUsuarios(UserManager<Usuario> userManager, IHttpContextAccessor contextAccessor)
        {
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
        }

        public async Task<Usuario?> ObtenerUsuario()
        {
            var email = contextAccessor.HttpContext?
               .User?
               .FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            return await userManager.FindByEmailAsync(email);
        }
    }
}
