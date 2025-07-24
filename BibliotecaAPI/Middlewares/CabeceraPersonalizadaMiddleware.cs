using System.Runtime.CompilerServices;

namespace BibliotecaAPI.Middlewares
{
    public class CabeceraPersonalizadaMiddleware
    {
        private readonly RequestDelegate next;

        public CabeceraPersonalizadaMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Append("mi-cabecera", "valor");
            await next.Invoke(context);
        }
    }

    public static class CabeceraPersonalizadaMiddlewareExtensions
    {
        public static IApplicationBuilder UseCabeceraPersonalizada(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CabeceraPersonalizadaMiddleware>();
        }
    }
}
