using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Comentario
    {
        public Guid Id { get; set; }
        [Required]
        public required string Cuerpo { get; set; }
        public int LibroId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Libro? Libro { get; set; }
        public required string UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public bool EstaBorrado { get; set; }
    }
}
