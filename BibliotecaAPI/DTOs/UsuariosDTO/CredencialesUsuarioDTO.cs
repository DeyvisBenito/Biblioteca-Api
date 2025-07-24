using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.UsuariosDTO
{
    public class CredencialesUsuarioDTO
    {
        [Required]
        [EmailAddress(ErrorMessage ="Debe ser un Email valido")]
        public required string Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
