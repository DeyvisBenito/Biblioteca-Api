using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace BibliotecaAPI.Utilidades
{
    public class TiempoDeEjecucionFiltro : IAsyncActionFilter
    {
        private readonly ILogger<TiempoDeEjecucionFiltro> logger;

        public TiempoDeEjecucionFiltro(ILogger<TiempoDeEjecucionFiltro> logger)
        {
            this.logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Antes de la ejecucion de la accion
            var watch = Stopwatch.StartNew();
            logger.LogInformation($"Inicio de la ejecucion de la accion {context.ActionDescriptor.DisplayName}");

            await next();
            //Despues de la ejecucion de la accion
            watch.Stop();
            logger.LogInformation($"Fin de la ejecucion de la accion {context.ActionDescriptor.DisplayName}: " +
                     $"{watch.ElapsedMilliseconds} -ms");
        }
    }
}
