using BibliotecaAPI.Entidades;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class LibroDTO
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
    }
}
