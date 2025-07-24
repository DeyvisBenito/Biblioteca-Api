using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Interfaces
{
    public interface IServicioHash
    {
        HashDTO Hash(string texto);
        HashDTO Hash(string texto, byte[] sal);
    }
}
