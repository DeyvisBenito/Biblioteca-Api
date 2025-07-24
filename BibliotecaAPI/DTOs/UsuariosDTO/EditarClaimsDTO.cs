using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.UsuariosDTO
{
    public class EditarClaimsDTO
    {
        [EmailAddress(ErrorMessage ="Email invalido")]
        [Required]
        public required string Email { get; set; }
    }
}
