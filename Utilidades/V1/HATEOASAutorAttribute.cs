using BibliotecaAPI.DTOs;
using BibliotecaAPI.Servicios.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades.V1
{
    public class HATEOASAutorAttribute: HATEOASFilterAtribute
    {
        private readonly IGeneradorEnlaces generadorEnlaces;

        public HATEOASAutorAttribute(IGeneradorEnlaces generadorEnlaces)
        {
            this.generadorEnlaces = generadorEnlaces;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var incluirHATOAS = DebeIncluirHATEOAS(context);

            if (!incluirHATOAS)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var modelo = result!.Value as AutorDTO ??
                throw new ArgumentNullException("Se esperaba una instancia de AutorDTO");

            await generadorEnlaces.GenerarEnlaces(modelo);
            await next();
        }
    }
}
