using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace BibliotecaAPITests.Pruebas_de_Integracion.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas: BasePruebasDBContextIMapper
    {
        private readonly string url = "api/v1/autores";
        private string db = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Get_Retorna404_SiAutorNoExiste()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(db);
            var cliente = factory.CreateClient();

            //Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            //Validacion
            var resultado = respuesta.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: resultado);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_SiAutorExiste()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(db);
            var cliente = factory.CreateClient();

            var context = ConstruirContext(db);
            context.Autores.Add(new Autor
            {
                Nombres = "Deyvis Saul",
                Apellidos = "Benito Medrano"
            });
            await context.SaveChangesAsync();

            //Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            //Validacion
            respuesta.EnsureSuccessStatusCode();

            var autor = JsonSerializer.Deserialize<AutorConLibrosDTO>(
                 await respuesta.Content.ReadAsStringAsync(), jsonSerializerOptions
                );

            Assert.AreEqual(expected: 1, actual: autor!.Id);
        }

        [TestMethod]
        public async Task Post_Retorna401_SiNoTieneAutorizacion()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(db, ignorarSeguridad: false);
            var cliente = factory.CreateClient();

            var autorCreacion = new AutorCreacionDTO
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacion);

            // Validacion
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Retorna403_SiUsuarioNoEsAdmin()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(db, ignorarSeguridad: false);
            var cliente = factory.CreateClient();

            var token = await CrearUsuario(db, factory);

            cliente.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacion = new AutorCreacionDTO
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };
            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacion);


            // Validacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Retorna201_SiUsuarioEsAdmin()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(db, ignorarSeguridad: false);
            var cliente = factory.CreateClient();

            var claims = new List<Claim>
            {
                AdminClaim
            };

            var token = await CrearUsuario(db, factory, claims);

            cliente.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacion = new AutorCreacionDTO
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacion);

            // Validacion
            respuesta.EnsureSuccessStatusCode();

            Assert.AreEqual(expected: HttpStatusCode.Created, actual: respuesta.StatusCode);
        }
    }
}
