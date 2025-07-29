using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.Interfaces
{
    public interface IServicioLlaveAPI
    {
        Task<LlaveApi> CrearLlaveAPI(string usuarioId, TipoLlave tipoLlave);
        string GenerarLlave();
    }
}
