using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Interfaces
{
    public interface IGenerarEnlaces
    {
        public Task GenerarRutasAutor(AutorDTO autorDTo);
        public Task<RutasDeColeccionDTO<AutorDTO>> GenerarRutasAutores(List<AutorDTO> autores);
    }
}
