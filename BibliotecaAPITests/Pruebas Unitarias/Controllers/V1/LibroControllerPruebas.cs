using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.Pruebas_Unitarias.Controllers.V1
{
    [TestClass]
    public class LibroControllerPruebas: BasePruebasDBContextIMapper
    {
        private string db = null!;
        IDataProtectionProvider protectionProvider = null!;
        IOutputCacheStore outputCachStore = null!;
        ITimeLimitedDataProtector protectorPorTiempo = null!;
        LibroController controller = null!;

        [TestInitialize]
        public void Setup()
        {
            db = Guid.NewGuid().ToString();
            var context = ConstruirContext(db);
            var mapper = ConstruirMapper();
            protectionProvider = Substitute.For<IDataProtectionProvider>();
            outputCachStore = Substitute.For<IOutputCacheStore>();
            protectorPorTiempo = protectionProvider.CreateProtector("LibroController").ToTimeLimitedDataProtector();

            controller = new LibroController(context, mapper, protectionProvider, outputCachStore);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [TestMethod]
        public async Task Get_RetornaCeroLibros_CuandoNoHayLibros()
        {
            //Preparacion
            var paginacion = new PaginacionDTO()
            {
                Pagina = 1,
                CantidadRecordsPorPagina = 1
            };

            //Prueba
            var libros = await controller.Get(paginacion);

            //Validacion
            Assert.AreEqual(expected: 0, actual: libros.Count());
        }
    }
}
