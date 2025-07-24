using BibliotecaAPI.DTOs;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades.V1
{
    public class HateoasAutoresAttribute: HATEOASFilterAttribute
    {
        private readonly IGenerarEnlaces generarEnlaces;

        public HateoasAutoresAttribute(IGenerarEnlaces generarEnlaces)
        {
            this.generarEnlaces = generarEnlaces;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var requiereHateoas = DebeIncluirHateoas(context);
            if (!requiereHateoas)
            {
                await next();
                return;
            }

            var resultado = context.Result as ObjectResult;
            var modelo = resultado!.Value as List<AutorDTO> ?? throw new ArgumentNullException("Se espera una lista de AutorDTO");

            context.Result = new OkObjectResult(await generarEnlaces.GenerarRutasAutores(modelo));
            await next();
        }
    }
}
