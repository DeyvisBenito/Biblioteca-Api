namespace BibliotecaAPI.DTOs
{
    public class AutorConLibrosDTO: AutorDTO
    {
        public IEnumerable<LibroDTO> Libros { get; set; } = [];
    }
}
