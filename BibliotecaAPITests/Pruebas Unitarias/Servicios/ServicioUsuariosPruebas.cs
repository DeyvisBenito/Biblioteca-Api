using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.Pruebas_Unitarias.Servicios
{
    [TestClass]
    public class ServicioUsuariosPruebas
    {
        UserManager<Usuario> userManager = null!;
        IHttpContextAccessor contextAccesor = null!;
        ServicioUsuarios servicioUsuarios = null!;

        [TestInitialize]
        public void Setup()
        {
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null);

            contextAccesor = Substitute.For<IHttpContextAccessor>();
            servicioUsuarios = new ServicioUsuarios(userManager, contextAccesor);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNull_SiNoEncuentraClaimEmail()
        {
            //Preparacion
            var httpcontext = new DefaultHttpContext();
            contextAccesor.HttpContext.Returns(httpcontext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            //Validacion
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNull_SiUsuarioConClaimEmailNoExiste()
        {
            //Preparacion
            var email = "prueba@gmail.com";

            userManager.FindByEmailAsync(email).Returns(Task.FromResult<Usuario?>(null));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpcontext = new DefaultHttpContext() { User = claims };
            contextAccesor.HttpContext.Returns(httpcontext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            //Validacion
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaUsuario_SiEncuentraUsuarioConClaimEmail()
        {
            //Preparacion
            var email = "prueba@gmail.com";
            var usuarioEsperado = new Usuario()
            {
                Email = email
            };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(usuarioEsperado));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext()
            {
                User = claims
            };
            contextAccesor.HttpContext.Returns(httpContext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            //Validacion
            Assert.IsNotNull(usuario);
            Assert.AreEqual(email, usuario.Email);
        }
    }
}
