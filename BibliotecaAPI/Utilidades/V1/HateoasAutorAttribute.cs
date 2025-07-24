using BibliotecaAPI.DTOs;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades.V1
{
    public class HateoasAutorAttribute: HATEOASFilterAttribute
    {
        private readonly IGenerarEnlaces generarEnlaces;

        public HateoasAutorAttribute(IGenerarEnlaces generarEnlaces)
        {
            this.generarEnlaces = generarEnlaces;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var incluirHateos = DebeIncluirHateoas(context);

            if (!incluirHateos)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var modelo = result!.Value as AutorDTO ?? throw new ArgumentNullException("Se espera un AutorDTO");

            await generarEnlaces.GenerarRutasAutor(modelo);
            await next();
        }
    }
}
