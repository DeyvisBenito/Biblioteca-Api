using AutoMapper;
using Azure;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using BibliotecaAPI.Servicios;
using BibliotecaAPITests.Utilidades;
using BibliotecaAPITests.Utilidades.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.Pruebas_Unitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebasDBContextIMapper
    {
        IAlmacenarArchivos almacenarArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore cache = null!;
        AutoresController controller = null!;
        private string db = null!;

        private const string contenedor = "autores";
        private const string cacheAutores = "autores-obtenerV1";

        [TestInitialize]
        public void Setup()
        {
            db = Guid.NewGuid().ToString();
            var context = ConstruirContext(db);
            var mapper = ConstruirMapper();
            almacenarArchivos = Substitute.For<IAlmacenarArchivos>();
            logger = Substitute.For<ILogger<AutoresController>>();
            cache = Substitute.For<IOutputCacheStore>();

            controller = new AutoresController(context, mapper, almacenarArchivos, logger, cache);
        }


        [TestMethod]
        public async Task Get_ObtieneAutores_MandandolePaginacion()
        {
            //Aca dejo plantilla de utilizar servicios con Substitute

            //Preparacion de prueba       
            //Mock para usar servicio Autores
            // IServicioAutores servicioAutores = Substitute.For<IServicioAutores>();



            //Mandando el HttpContext falso porque eso solo viene de solicitudes
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var paginacionDTO = new PaginacionDTO(3, 2);

            //Prueba
            var respuesta = await controller.Get(paginacionDTO);
            //Prueba del Mock o uso de servicio o interfaz
            //await controller.Get(paginacionDTO)

            //Validacion
            var resultado = respuesta as OkObjectResult;
            Assert.IsNotNull(resultado);

            //Validacion del Mock, esto debe de coincidir con las llamadas '1' al servicio y lo enviado en paginacionDTO
            // await servicioAutores.Received(1).Get(paginacionDTO);
        }

        [TestMethod]
        public async Task Get_RetornaNotFound_SiAutorNoExisteID()
        {
            //Preparacion de prueba

            //Probar
            var autor = await controller.Get(1);
            var resultado = autor.Result as ObjectResult;
            var codigo = resultado!.StatusCode;


            //Validacion
            Assert.AreEqual(expected: 404, actual: codigo);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_SiEncuentraAutorConId()
        {
            //Preparacion de prueba
            var context = ConstruirContext(db);

            var autor = new Autor()
            {
                Nombres = "Deyvis Saul",
                Apellidos = "Benito Medrano"
            };
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            //Ejecucion de prueba
            var respuestaAutor = await controller.Get(1);
            var resultado = respuestaAutor.Result as ObjectResult;
            var autorResultado = resultado!.Value as AutorConLibrosDTO;

            //Validacion           
            Assert.AreEqual(expected: 1, actual: autorResultado!.Id);
        }

        [TestMethod]
        public async Task Post_InsertaUnAutor_AlEnviarleUnAutor()
        {
            //Preparacion
            var autorCreacion = new AutorCreacionDTO()
            {
                Nombres = "Autor",
                Apellidos = "Prueba"
            };

            //Prueba
            var respuesta = await controller.Post(autorCreacion);

            //Validacion
            //1.
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            //2.
            var context = ConstruirContext(db);
            var cantidad = await context.Autores.CountAsync();

            Assert.AreEqual(expected: 1, actual: cantidad);
        }

        [TestMethod]
        public async Task Put_Retorna404_SiAutorAAcutualizarNoExiste()
        {
            //Preparacion
            var autorCreacion = new AutorCreacionDTOConFoto
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };

            //Prueba
            var respuesta = await controller.Put(1, autorCreacion);

            //Validacion
            var resultado = respuesta as ObjectResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Put_RetornaNoContent_AlActualizarAutorSinFoto()
        {
            //Preparacion
            var autor = new Autor()
            {
                Nombres = "Deyvis",
                Apellidos = "Benito",
                Identificacion = "Id"
            };
            var context = ConstruirContext(db);
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var autorActualizar = new AutorCreacionDTOConFoto()
            {
                Nombres = "Deyvis2",
                Apellidos = "Benito2",
                Identificacion = "Id2"
            };

            //Prueba
            var respuesta = await controller.Put(1, autorActualizar);
            var resultado = respuesta as NoContentResult;

            //Validacion
            Assert.AreEqual(204, resultado!.StatusCode);
            await almacenarArchivos.DidNotReceiveWithAnyArgs().Actualizar(default, default!, default!);
            await cache.Received(1).EvictByTagAsync(cacheAutores, default);

            var context2 = ConstruirContext(db);
            var autorActualizado = await context2.Autores.SingleAsync();
            Assert.AreEqual("Deyvis2", autorActualizado.Nombres);
            Assert.AreEqual("Benito2", autorActualizado.Apellidos);
            Assert.AreEqual("Id2", autorActualizado.Identificacion);
        }

        [TestMethod]
        public async Task Put_RetornaNoContent_AlActualizarAutorConFoto()
        {
            //Preparacion
            var rutaAnterior = "url1";
            var rutaNueva = "url2";

            var autor = new Autor()
            {
                Nombres = "Deyvis",
                Apellidos = "Benito",
                Foto = rutaAnterior
            };

            var context = ConstruirContext(db);
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();
            var autorActualizar = new AutorCreacionDTOConFoto()
            {
                Nombres = "Deyvis2",
                Apellidos = "Benito2",
                Foto = formFile
            };

            almacenarArchivos.Actualizar(default, default!, default!).ReturnsForAnyArgs(rutaNueva);


            //Prueba
            var respuesta = await controller.Put(1, autorActualizar);

            //Validacion
            var resultado = respuesta as NoContentResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var contexto2 = ConstruirContext(db);
            var autorActualizado = await contexto2.Autores.SingleAsync();
            Assert.AreEqual("Deyvis2", autorActualizado.Nombres);
            Assert.AreEqual("Benito2", autorActualizado.Apellidos);
            Assert.AreEqual(rutaNueva, autorActualizado.Foto);

            await almacenarArchivos.Received(1).Actualizar(rutaAnterior, contenedor, autorActualizar.Foto);
            await cache.Received(1).EvictByTagAsync(cacheAutores, default);
        }

        [TestMethod]
        public async Task Patch_Retorna400_SiJSONPatchEsNulo()
        {
            //Preparacion

            //Prueba
            var respuesta = await controller.Patch(1, jsonPatchDoc: null!);

            //Validacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(400, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_Retorna404_SiAutorNoExiste()
        {
            //Preparacion
            var jsonPatchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //Prueba
            var respuesta = await controller.Patch(1, jsonPatchDoc);

            //Validacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_RetornaValidationProblem_SiJSONPatchNoEsValido()
        {
            //Preparacion
            var autor = new Autor()
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };
            var context = ConstruirContext(db);
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var jsonPatchDoc = new JsonPatchDocument<AutorPatchDTO>();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeDeError = "Ha habido un error en el modelo de autor";
            controller.ModelState.AddModelError("", mensajeDeError);

            //Prueba
            var respuesta = await controller.Patch(1, jsonPatchDoc);

            //Validacion
            var resultado = respuesta as ObjectResult;
            var modelError = resultado!.Value as ValidationProblemDetails;

            Assert.IsNotNull(modelError);
            Assert.AreEqual(1, modelError.Errors.Keys.Count);
            Assert.AreEqual(mensajeDeError, modelError.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Patch_Retorna204_SiPatchFueExitoso()
        {
            //Preparacion
            var autor = new Autor()
            {
                Nombres = "Deyvis",
                Apellidos = "Benito"
            };
            var context = ConstruirContext(db);
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var jsonPatchDoc = new JsonPatchDocument<AutorPatchDTO>();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            jsonPatchDoc.Operations.Add(new Microsoft.AspNetCore.JsonPatch.Operations.Operation<AutorPatchDTO>
                                            ("replace", "/Nombres", null, "Deyvis2"));

            //Prueba
            var respuesta = await controller.Patch(1, jsonPatchDoc);

            //Validacion
            await cache.Received(1).EvictByTagAsync(cacheAutores, default);

            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var contexto2 = ConstruirContext(db);
            var autorPatch = await contexto2.Autores.SingleAsync();

            Assert.AreEqual("Deyvis2", autorPatch.Nombres);
            Assert.AreEqual("Benito", autorPatch.Apellidos);

        }

        [TestMethod]
        public async Task Delete_Retorna404_SiAutorNoExiste()
        {
            //Preparacion

            //Prueba
            var respuesta = await controller.Delete(1);

            //Validacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_BorraAutor_SiAutorExiste()
        {
            //Preparacion
            var context = ConstruirContext(db);
            var autor = new Autor()
            {
                Nombres = "Deyvis",
                Apellidos = "Benito",
                Foto = "URL-1"
            };
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            //Prueba
            var respuesta = await controller.Delete(1);

            //Validacion
            await cache.Received(1).EvictByTagAsync(cacheAutores, default);
            await almacenarArchivos.Received(1).Borrar(autor.Foto, contenedor);

            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context2 = ConstruirContext(db);
            var autorBD = await context2.Autores.SingleOrDefaultAsync();
            Assert.IsNull(autorBD);
        }
    }
}
