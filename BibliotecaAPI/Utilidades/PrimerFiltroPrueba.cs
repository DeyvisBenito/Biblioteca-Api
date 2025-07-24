using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class PrimerFiltroPrueba : IActionFilter
    {
        private readonly ILogger<PrimerFiltroPrueba> logger;

        public PrimerFiltroPrueba(ILogger<PrimerFiltroPrueba> logger)
        {
            this.logger = logger;
        }

        //Filtro antes de la accion
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Ejecutando antes de la accion");
        }

        //Filtro despues de la accion
        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("Ejecutando despues de la accion");
        }
    }
}
