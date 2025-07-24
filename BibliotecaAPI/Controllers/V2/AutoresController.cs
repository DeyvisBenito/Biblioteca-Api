using AutoMapper;
using Azure;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/autores")]
    [Authorize(Policy = "EsAdmin")]
    [FiltroAgregarCabecera("Controlador", "Autores")]
    public class AutoresController: ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenarArchivos almacenarArchivos;
        private readonly ILogger logger;
        private readonly IOutputCacheStore outputCacheStore;
        private const string contenedor = "autores";
        private const string cache = "autores-obtenerV2";

        public AutoresController(ApplicationDBContext context, IMapper mapper, IAlmacenarArchivos almacenarArchivos, 
            ILogger<AutoresController> logger, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenarArchivos = almacenarArchivos;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<PrimerFiltroPrueba>()]
        [FiltroAgregarCabecera("Accion", "ObtenerAutores")]
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var quearyble = context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(quearyble);
            var autores = await quearyble.OrderBy(x => x.Nombres).Paginar(paginacionDTO).ToListAsync();
            var autorDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);

            return autorDTO;
        }


        [HttpGet("{Id:int}", Name = "ObtenerAutorV2")]
        [AllowAnonymous]
        [EndpointSummary("Obtiene un autor por su ID")]
        [EndpointDescription("Obtiene un autor por su ID y si no lo encuentra devuelve un 404 not found")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<AutorConLibrosDTO>> Get([Description("Id del autor")]int Id, bool incluirLibros)
        {
            var queryable = context.Autores.AsQueryable();
            if (incluirLibros)
            {
                queryable = queryable.Include(x => x.Libros).ThenInclude(x => x.Libro);
            }

            var autor = await queryable.FirstOrDefaultAsync(x => x.Id == Id);

            if(autor is null)
            {
                return NotFound("Autor no encontrado");
            }
            var autorConLibrosDTO = mapper.Map<AutorConLibrosDTO>(autor);

            return autorConLibrosDTO;
                
        }

        [HttpGet("filtrar")]
        [AllowAnonymous]
        public async Task<ActionResult> GetFiltrar([FromQuery] FiltrarAutoresDTO filtrarAutoresDTO)
        {
            var quearyble = context.Autores.AsQueryable();

            if (!string.IsNullOrEmpty(filtrarAutoresDTO.Nombres))
            {
                quearyble = quearyble.Where(x => x.Nombres.Contains(filtrarAutoresDTO.Nombres));
            }
            if (!string.IsNullOrEmpty(filtrarAutoresDTO.Apellidos))
            {
                quearyble = quearyble.Where(x => x.Apellidos.Contains(filtrarAutoresDTO.Apellidos));
            }
            if (filtrarAutoresDTO.TieneFoto.HasValue)
            {
                if (filtrarAutoresDTO.TieneFoto.Value)
                {
                    quearyble = quearyble.Where(x => x.Foto != null);
                }
                else
                {
                    quearyble = quearyble.Where(x => x.Foto == null);
                }
            }
            if (filtrarAutoresDTO.TieneLibros.HasValue)
            {
                if (filtrarAutoresDTO.TieneLibros.Value)
                {
                    quearyble = quearyble.Where(x => x.Libros.Any());
                }
                else
                {
                    quearyble = quearyble.Where(x => !x.Libros.Any());
                }
            }
            if (!string.IsNullOrEmpty(filtrarAutoresDTO.TituloLibro))
            {
                quearyble = quearyble.Where(x => x.Libros.Any(y => y.Libro!.Nombre.Contains(filtrarAutoresDTO.TituloLibro)));
            }
            if (filtrarAutoresDTO.IncluirLibros)
            {
                quearyble = quearyble.Include(x => x.Libros).ThenInclude(x => x.Libro);
            }
            if (!string.IsNullOrEmpty(filtrarAutoresDTO.CampoOrdenar))
            {
                var tipoOrden = filtrarAutoresDTO.OrdenarAscendente ? "ascending" : "descending";

                try
                {
                    quearyble = quearyble.OrderBy($"{filtrarAutoresDTO.CampoOrdenar} {tipoOrden}");
                }catch(Exception ex)
                {
                    quearyble = quearyble.OrderBy(x => x.Nombres);
                    logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                quearyble = quearyble.OrderBy(x => x.Nombres);
            }

                var autores = await quearyble.Paginar(filtrarAutoresDTO.PaginacionDTO).ToListAsync();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(quearyble);
            if (filtrarAutoresDTO.IncluirLibros)
            {
                var autoresConLibrosDTO = mapper.Map<IEnumerable<AutorConLibrosDTO>>(autores);
                return Ok(autoresConLibrosDTO);
            }
            else
            {
                var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoresDTO);
            }         
        }

        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacion)
        {
            var autor = mapper.Map<Autor>(autorCreacion);
            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV2", new { autor.Id }, autorDTO);
        }

        [HttpPost("con-foto")]
        public async Task<ActionResult> PostConFoto([FromForm] AutorCreacionDTOConFoto autorCreacion)
        {
            var autor = mapper.Map<Autor>(autorCreacion);

            if(autorCreacion.Foto is not null)
            {
                var ruta = await almacenarArchivos.Almacenar(contenedor, autorCreacion.Foto);
                autor.Foto = ruta;
            }

            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV2", new { autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id,[FromForm] AutorCreacionDTOConFoto autorCreacion)
        {
            var autor = mapper.Map<Autor>(autorCreacion);

            autor.Id = id;
            var autorExist = await context.Autores.AnyAsync(x => x.Id == id);
            if (!autorExist)
            {
                return NotFound("El autor a actualizar no existe");
            }

            if(autorCreacion.Foto is not null)
            {
                var rutaActual = await context.Autores.Where(x => x.Id == id).Select(x => x.Foto).FirstAsync();
                var ruta = await almacenarArchivos.Actualizar(rutaActual, contenedor, autorCreacion.Foto);
                autor.Foto = ruta;
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AutorPatchDTO> jsonPatchDoc)
        {
            if(jsonPatchDoc is null)
            {
                return BadRequest();
            }

            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if(autor is null)
            {
                return NotFound();
            }

            var autorPatch = mapper.Map<AutorPatchDTO>(autor);

            jsonPatchDoc.ApplyTo(autorPatch, ModelState);
            var isValid = TryValidateModel(autorPatch);

            if (!isValid)
            {
                return ValidationProblem();
            }

            mapper.Map(autorPatch, autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            await almacenarArchivos.Borrar(autor.Foto, contenedor);

            return NoContent();
        }
    }
}
