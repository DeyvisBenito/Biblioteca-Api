using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class ComentarioDTO
    {
        public Guid Id { get; set; }
        public required string Cuerpo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public required string  UsuarioId { get; set; }
        public required string UsuarioEmail { get; set; }

    }
}
