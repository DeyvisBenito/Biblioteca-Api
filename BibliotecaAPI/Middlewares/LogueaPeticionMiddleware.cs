using Microsoft.AspNetCore.Builder;

namespace BibliotecaAPI.Middlewares
{
    public class LogueaPeticionMiddleware
    {
        private readonly RequestDelegate next;

        public LogueaPeticionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            //Viene peticion
            var logger = contexto.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"Peticion: {contexto.Request.Method} {contexto.Request.Path}");

            //Siguiente middleware
            await next.Invoke(contexto);

            //Vuelta, envio de respuesta
            logger.LogInformation($"Respuesta: {contexto.Response.StatusCode}");
        }
    }

    public static class LogueaPeticionMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogueoPeticion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogueaPeticionMiddleware>();
        }
    }
}
