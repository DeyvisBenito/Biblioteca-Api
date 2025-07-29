using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;

namespace BibliotecaAPI.Servicios
{
    public class ServicioLlaveAPI : IServicioLlaveAPI
    {
        private readonly ApplicationDBContext context;

        public ServicioLlaveAPI(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<LlaveApi> CrearLlaveAPI(string usuarioId, TipoLlave tipoLlave)
        {
            var llave = GenerarLlave();
            var llaveApi = new LlaveApi
            {
                Llave = llave,
                TipoLlave = tipoLlave,
                Activa = true,
                UsuarioId = usuarioId
            };
            context.LlavesAPI.Add(llaveApi);
            await context.SaveChangesAsync();

            return llaveApi;
        }

        public string GenerarLlave() => Guid.NewGuid().ToString().Replace("-", "");
    }
}
