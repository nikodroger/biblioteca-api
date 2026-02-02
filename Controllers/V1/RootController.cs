using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RootController: ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRootV1")]
        [AllowAnonymous]
        public async Task<IEnumerable<DatosHATEOASDTO>> Get()
        {
            var datosHATEOAS = new List<DatosHATEOASDTO>();

            var esAdmin = await authorizationService.AuthorizeAsync(User, "esadmin");

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerRootV1", new { })!,
                Descripcion: "self", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerAutoresV1", new { })!,
                Descripcion: "autores-obtener", Metodo: "GET"));           

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerLibrosV1", new { })!,
                Descripcion: "libros-obtener", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearUsuarioV1", new { })!,
                    Descripcion: "usuario-crear", Metodo: "POST"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("LoginUsuarioV1", new { })!,
                Descripcion: "usuario-LOGIN", Metodo: "POST"));

            

            if (User.Identity!.IsAuthenticated)
            {
                // Accciones para usuarios logueados

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("RenovarTokenUsuarioV1", new { })!,
                    Descripcion: "token-renovar", Metodo: "GET"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ActualizarUsuarioV1", new { })!,
                    Descripcion: "usuario-actualizar", Metodo: "PUT"));

            }


            // Acciones que sólo usuarios admins pueden realizar

            if (esAdmin.Succeeded)
            {
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutoresV1", new { })!,
                    Descripcion: "autores-crear", Metodo: "POST"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorV1", new { })!,
                    Descripcion: "autor-crear", Metodo: "POST"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearLibroV1", new { })!,
                    Descripcion: "libro-crear", Metodo: "POST"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerUsuariosV1", new { })!,
                    Descripcion: "usuarios-obtener", Metodo: "GET"));

                
            }


            return datosHATEOAS; 
        }
    }
}
