using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class LibroCreacionDTO
    {
        [Required]
        public required string Nombre { get; set; }
        public List<int> AutoresId { get; set; } = [];
    }
}
