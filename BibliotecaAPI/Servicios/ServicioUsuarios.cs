using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Servicios
{
    public class ServicioUsuarios: IServicioUsuarios
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IHttpContextAccessor contextAccessor;

        public ServicioUsuarios(UserManager<Usuario> userManager, IHttpContextAccessor contextAccessor)
        {
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
        }

        public async Task<Usuario?> ObtenerUsuario()
        {
            var emailClaim = contextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "email");

            if(emailClaim is null)
            {
                return null;
            }

            var usuario = await userManager.FindByEmailAsync(emailClaim!.Value);

            if(usuario is null)
            {
                return null;
            }

            return usuario;
        }
    }
}
