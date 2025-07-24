using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.DTOs
{
    public class LibroConAutorDTO: LibroDTO
    {
        public List<AutorDTO> Autores { get; set; } = [];
        
    }
}
