using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;

namespace BibliotecaAPITests.Pruebas_de_Integracion.Controllers.V1
{
    [TestClass]
    public class ComentariosControllerPruebas : BasePruebasDBContextIMapper
    {
        private readonly string db = Guid.NewGuid().ToString();
        private readonly string url = "api/v1/ibros/1/comentarios";

        private async Task PrepararElementos()
        {
            var context = ConstruirContext(db);

            var autor = new Autor
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var libro = new Libro
            {
                Nombre = "Libro de Deyvis",
                Autores = new List<AutorLibro>()
                {
                    new AutorLibro{ Autor = autor }
                }
            };

            context.Libros.Add(libro);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Retorna204_SiUsuarioEliminaSuPropioComentario()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(db, ignorarSeguridad: false);
            var cliente = factory.CreateClient();
            var token = await CrearUsuario(db, factory);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await PrepararElementos();

            var context = ConstruirContext(db);

            var usuario = await context.Users.FirstOrDefaultAsync();

            var comentario = new Comentario
            {
                Cuerpo = "Cuerpo de comentario",
                UsuarioId = usuario!.Id,
                LibroId = 1
            };
            context.Comentarios.Add(comentario);
            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            // Validacion
            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Retorna403_SiUsuarioIntentaEliminarComentarioDeOtroUsuario()
        {
            // Preparacion
            var context = ConstruirContext(db);
            await PrepararElementos();

            var factory = ConstruirWebApplicationFactory(db, ignorarSeguridad: false);
            var cliente = factory.CreateClient();

            await CrearUsuario(db, factory, [], "UserQueCreaComentario@gmail.com");
            var usuario = await context.Users.FirstOrDefaultAsync();

            var comentario = new Comentario
            {
                Cuerpo = "Cuerpo de comentario",
                UsuarioId = usuario!.Id,
                LibroId = 1
            };
            context.Comentarios.Add(comentario);
            await context.SaveChangesAsync();

            var token = await CrearUsuario(db, factory, [], "OtroUsuario@gmail.com");
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            // Validacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }
    }
}
