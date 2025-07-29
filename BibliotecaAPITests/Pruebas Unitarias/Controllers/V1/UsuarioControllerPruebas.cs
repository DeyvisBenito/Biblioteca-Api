using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs.UsuariosDTO;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.Pruebas_Unitarias.Controllers.V1
{
    [TestClass]
    public class UsuarioControllerPruebas : BasePruebasDBContextIMapper
    {
        private string db = null!;
        UserManager<Usuario> userManager = null!;
        private SignInManager<Usuario> signIngManager = null!;
        private UsuarioController controller = null!;
        IServicioLlaveAPI servicioLlaveApi = null!;

        [TestInitialize]
        public void Setup()
        {
            db = Guid.NewGuid().ToString();
            var context = ConstruirContext(db);
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null);

            var miConfiguration = new Dictionary<string, string>
            {
                {
                    "llavejwt", "asdnafasjkjASDANSKDASKDAJNSCASJKDA"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(miConfiguration!)
                .Build();

            var contextAccesor = Substitute.For<IHttpContextAccessor>();
            var userClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<Usuario>>();

            signIngManager = Substitute.For<SignInManager<Usuario>>(userManager,
                contextAccesor, userClaimsFactory, null, null, null, null);

            var servicioUsuarios = Substitute.For<IServicioUsuarios>();
            var mapper = ConstruirMapper();
            servicioLlaveApi = Substitute.For<IServicioLlaveAPI>();

            controller = new UsuarioController(userManager, configuration, signIngManager, servicioUsuarios, context, mapper, servicioLlaveApi);
        }

        [TestMethod]
        public async Task Registrar_RetornaValidationProblems_CuandoOcurreUnError()
        {
            //Preparacion
            var mensajeError = "prueba";
            var userCredential = new CredencialesUsuarioDTO
            {
                Email = "prueba@gmail.com",
                Password = "aA123456!"
            };

            userManager.CreateAsync(Arg.Any<Usuario>(), Arg.Any<string>()).Returns(IdentityResult.Failed(new IdentityError
            {
                Code = "Prueba",
                Description = mensajeError
            }));

            //Prueba
            var respuesta = await controller.Registrar(userCredential);

            //Validacion
            var resultado = respuesta.Result as ObjectResult;
            var problemsDetails = resultado!.Value as ValidationProblemDetails;

            Assert.IsNotNull(problemsDetails);
            Assert.AreEqual(expected: 1, actual: problemsDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeError, actual: problemsDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Registrar_RetornaToken_CuandoEsExitoso()
        {
            //Preparacion
            var userCredential = new CredencialesUsuarioDTO
            {
                Email = "prueba@gmail.com",
                Password = "aA123456!"
            };

            userManager.CreateAsync(Arg.Any<Usuario>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            //Prueba
            var respuesta = await controller.Registrar(userCredential);

            //Validacion
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value!.Token);
        }

        [TestMethod]
        public async Task Login_RetornaLoginIncorrecto_CuandoUsuarioNoExiste()
        {
            //Preparacion
            var credencialesUsuario = new CredencialesUsuarioDTO
            {
                Email = "prueba@gmail.com",
                Password = "aA123456!"
            };

            userManager.FindByEmailAsync(credencialesUsuario.Email).Returns(Task.FromResult < Usuario? > (null));
            //Prueba
            var respuesta = await controller.Login(credencialesUsuario);

            //Validacion
            var resultado = respuesta.Result as ObjectResult;
            var validationProblem = resultado!.Value as ValidationProblemDetails;

            Assert.IsNotNull(validationProblem);
            Assert.AreEqual(expected: 1, actual: validationProblem!.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login incorrecto", actual: validationProblem.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_RetornaLoginIncorrecto_CuandoPasswordIncorrecto()
        {
            //Preparacion
            var credencialesUsuario = new CredencialesUsuarioDTO
            {
                Email = "prueba@gmail.com",
                Password = "aA123456!"
            };

            var usuario = new Usuario
            {
                Email = credencialesUsuario.Email
            };

            userManager.FindByEmailAsync(credencialesUsuario.Email).Returns(Task.FromResult<Usuario?>(usuario));

            signIngManager.CheckPasswordSignInAsync(usuario, credencialesUsuario.Password!, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            //Prueba
            var respuesta = await controller.Login(credencialesUsuario);

            //Validacion
            var resultado = respuesta.Result as ObjectResult;
            var validationProblem = resultado!.Value as ValidationProblemDetails;

            Assert.IsNotNull(validationProblem);
            Assert.AreEqual(expected: 1, actual: validationProblem!.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login incorrecto", actual: validationProblem.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_RetornaToken_CuandoLoginEsExitoso()
        {
            //Preparacion
            var credencialesUsuario = new CredencialesUsuarioDTO
            {
                Email = "prueba@gmail.com",
                Password = "aA123456!"
            };

            var usuario = new Usuario
            {
                Email = credencialesUsuario.Email
            };

            userManager.FindByEmailAsync(credencialesUsuario.Email).Returns(Task.FromResult<Usuario?>(usuario));

            signIngManager.CheckPasswordSignInAsync(usuario, credencialesUsuario.Password!, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            //Prueba
            var respuesta = await controller.Login(credencialesUsuario);

            //Validacion
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value!.Token);
        }
    }
}
