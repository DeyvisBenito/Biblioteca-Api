using BibliotecaAPI.DTOs;
using BibliotecaAPI.Interfaces;
using BibliotecaAPI.Migrations;
using Microsoft.AspNetCore.Authorization;
using System;

namespace BibliotecaAPI.Servicios.V1
{
    public class GenerarEnlaces: IGenerarEnlaces
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IAuthorizationService authorizationService;
        private readonly LinkGenerator linkGenerator;

        public GenerarEnlaces(IHttpContextAccessor contextAccessor, IAuthorizationService authorizationService,
                              LinkGenerator linkGenerator)
        {
            this.contextAccessor = contextAccessor;
            this.authorizationService = authorizationService;
            this.linkGenerator = linkGenerator;
        }

        public async Task GenerarRutasAutor(AutorDTO autorDTo)
        {
            var usuario = contextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "EsAdmin");

            GenerarRutas(autorDTo, esAdmin.Succeeded);
        }

        public async Task<RutasDeColeccionDTO<AutorDTO>> GenerarRutasAutores(List<AutorDTO> autores)
        {
            var usuario = contextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "EsAdmin");

            var valores = new RutasDeColeccionDTO<AutorDTO>
            {
                Valores = autores
            };

            foreach(var autor in valores.Valores)
            {
                GenerarRutas(autor, esAdmin.Succeeded);
            }

            valores.rutas.Add(new DatosHateoasDTO(
                    enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "ObtenerAutoresV1", new { })!,
                    descripcion: "self",
                    metodo: "GET"
                    ));

            if (esAdmin.Succeeded)
            {
                valores.rutas.Add(new DatosHateoasDTO(
                    enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "InsertarAutorV1", new { })!,
                    descripcion: "Autor-Insertar",
                    metodo: "POST"
                    ));

                valores.rutas.Add(new DatosHateoasDTO(
                    enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "InsertarAutorConFotoV1", new { })!,
                    descripcion: "Autor-Insertar-Con-Foto",
                    metodo: "POST"
                    ));
            }

            return valores;
        }

        private void GenerarRutas(AutorDTO autorDTO, bool esAdmin)
        {
            autorDTO.rutas.Add(new DatosHateoasDTO
            (
                enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "ObtenerAutorV1", new { id = autorDTO.Id })!,
                descripcion: "self",
                metodo: "GET"
            ));

            if (esAdmin)
            {
                autorDTO.rutas.Add(new DatosHateoasDTO
            (
                enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "ActualizarAutorV1", new { id = autorDTO.Id })!,
                descripcion: "Autor-Actualizar",
                metodo: "PUT"
            ));

                autorDTO.rutas.Add(new DatosHateoasDTO
                (
                    enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "PatchAutorV1", new { id = autorDTO.Id })!,
                    descripcion: "Autor-Patch",
                    metodo: "PATCH"
                ));

                autorDTO.rutas.Add(new DatosHateoasDTO
                (
                    enlace: linkGenerator.GetUriByRouteValues(contextAccessor.HttpContext!, "Delete-AutorV1", new { id = autorDTO.Id })!,
                    descripcion: "Delete-Autor",
                    metodo: "DELETE"
                ));
            }         
        }

        
    }
}
