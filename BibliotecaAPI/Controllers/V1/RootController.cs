using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RootController : ControllerBase
    {
        private readonly IAuthorizationService authorizationService;
        private const string cache = "root";

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRutasV1")]
        public async Task<IEnumerable<DatosHateoasDTO>> Get()
        {
            var esAdmin = await authorizationService.AuthorizeAsync(User, "EsAdmin");

            //Rutas para Hateaos, son rutas relevantes o generales, no todas

            var datosHateoas = new List<DatosHateoasDTO>();

    //Rutas para no logueados
            //Ruta actual
            datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("ObtenerRutasV1", new { })!,
                descripcion: "self",
                metodo: "GET"
                ));

            datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("ObtenerAutoresV1", new { })!,
                descripcion: "Obtener Autores",
                metodo: "GET"));

            datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("FiltrarAutoresV1", new { })!,
                descripcion: "Obtener Autores Filtrados",
                metodo: "GET"));

            datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("ObtenerLibrosV1", new { })!,
                descripcion: "Obtener Libros",
                metodo: "GET"));

            datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("InsertarUsuarioV1", new { })!,
                descripcion: "Insertar Usuario",
                metodo: "POST"));

            datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("LoginV1", new { })!,
                descripcion: "Login",
                metodo: "POST"));



            //Rutas para logueados
            if (User.Identity!.IsAuthenticated)
            {
                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("RenovarTokenV1", new { })!,
                    descripcion: "Renovar Token",
                    metodo: "GET"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("ActualizarUsuarioV1", new { })!,
                    descripcion: "Actualizar Usuario Actual",
                    metodo: "PUT"));
            }

            //Rutas para Admin
            if (esAdmin.Succeeded)
            {
                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("ObtenerUsuariosV1", new { })!,
                    descripcion: "Obtener Usuarios",
                    metodo: "GET"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("InsertarLibroV1", new { })!,
                    descripcion: "Insertar Libro",
                    metodo: "POST"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("HashV1", new { })!,
                    descripcion: "Encriptar con Hash",
                    metodo: "GET"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("EncriptarTextoV1", new { })!,
                    descripcion: "Encriptar con protector",
                    metodo: "GET"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("DesencriptarTextoV1", new { })!,
                    descripcion: "Desencriptar con protector",
                    metodo: "GET"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("InsertarAutorV1", new { })!,
                    descripcion: "Insertar Autor",
                    metodo: "POST"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("InsertarAutorConFotoV1", new { })!,
                    descripcion: "Insertar Autor con Foto",
                    metodo: "POST"));

                datosHateoas.Add(new DatosHateoasDTO(enlace: Url.Link("InsertarAutoresV1", new { })!,
                    descripcion: "Insertar autores coleccion",
                    metodo: "POST"));
            }

            return datosHateoas;
        }
    }
}
