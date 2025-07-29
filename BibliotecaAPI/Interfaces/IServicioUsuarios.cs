using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Interfaces
{
    public interface IServicioUsuarios
    {
        Task<Usuario?> ObtenerUsuario();
        string? ObtenerUsuarioId();
    }
}
