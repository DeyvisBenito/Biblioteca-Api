using BibliotecaAPI.DTOs;
using BibliotecaAPITests.Utilidades;
using System.Net;

namespace BibliotecaAPITests.Pruebas_de_Integracion.Controllers.V1
{
    [TestClass]
    public class LibroControllerPruebas: BasePruebasDBContextIMapper
    {
        private readonly string db = Guid.NewGuid().ToString();
        private readonly string url = "api/v1/libros";

        [TestMethod]
        public async Task Post_Retorna400_SiAutoresIdNoExisten()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(db);
            var cliente = factory.CreateClient();

            var libroCreacionDTO = new LibroCreacionDTO
            {
                Nombre = "Libro de prueba"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, libroCreacionDTO);

            // Validaciones
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: respuesta.StatusCode);
        }
    }
}
