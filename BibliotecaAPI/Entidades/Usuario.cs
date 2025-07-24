using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Entidades
{
    public class Usuario: IdentityUser
    {
        public DateTime fechaNacimiento { get; set; }
    }
}
