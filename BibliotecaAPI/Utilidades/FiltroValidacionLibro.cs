using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroValidacionLibro : IAsyncActionFilter
    {
        private readonly ApplicationDBContext dBContext;

        public FiltroValidacionLibro(ApplicationDBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(!context.ActionArguments.TryGetValue("libroCreacionDTO", out var valor) || 
                valor is not LibroCreacionDTO libroCreacionDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es valido");
                context.Result = context.ModelState.ConstruirProblemDetails();
                return;
            }

            if (libroCreacionDTO.AutoresId is null || libroCreacionDTO.AutoresId.Count == 0)
            {
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresId), "No se puede crear el libro sin autores");
                context.Result = context.ModelState.ConstruirProblemDetails();
                return;
            }

            var autoresExisten = await dBContext.Autores.Where(x => libroCreacionDTO.AutoresId.Contains(x.Id))
                                                      .Select(x => x.Id).ToListAsync();
            if (libroCreacionDTO.AutoresId.Count() != autoresExisten.Count)
            {
                var autoresNoValidos = libroCreacionDTO.AutoresId.Except(autoresExisten);
                var mensajeAutoresNoValidos = string.Join(",", autoresNoValidos);
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresId), $"Los siguientes autores no existen: {mensajeAutoresNoValidos}");
                context.Result = context.ModelState.ConstruirProblemDetails();
                return;
            }

            await next();
        }
    }
}
