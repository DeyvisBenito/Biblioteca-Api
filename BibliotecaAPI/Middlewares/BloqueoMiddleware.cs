using Microsoft.AspNetCore.Builder;

namespace BibliotecaAPI.Middlewares
{
    public class BloqueoMiddleware
    {
        private readonly RequestDelegate next;

        public BloqueoMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            if(contexto.Request.Path == "/bloqueado")
            {
                contexto.Response.StatusCode = 403;
                await contexto.Response.WriteAsync("Acceso denegado");
            }
            else
            {
                await next.Invoke(contexto);
            }
        }
    }

    public static class BloqueoMiddlewareExtensions
    {
        public static IApplicationBuilder UseBloqueo(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BloqueoMiddleware>();
        }
    }
}
