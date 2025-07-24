using AutoMapper;
using Azure;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/ibros/{libroId:int}/comentarios")]
    [Authorize]
    public class ComentariosController: ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "comentarios-obtenerV2";

        public ComentariosController(ApplicationDBContext context, IMapper mapper, IServicioUsuarios servicioUsuarios, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioUsuarios = servicioUsuarios;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var libroExist = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!libroExist)
            {
                return NotFound();
            }

            var cometarios = await context.Comentarios
                                   .Include(x => x.Usuario)
                                   .Where(x => x.LibroId == libroId)
                                   .OrderByDescending(x => x.FechaCreacion)
                                   .ToListAsync();

            return mapper.Map <List<ComentarioDTO>> (cometarios);
        }

        [HttpGet("{id}", Name = "ObtenerComentarioV2")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<ComentarioDTO>> Get(Guid id)
        {
            var comentario = await context.Comentarios.Include(x => x.Usuario).FirstOrDefaultAsync(x => x.Id == id);
            if(comentario is null)
            {
                return NotFound();
            }

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return comentarioDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var libroExist = await context.Libros.AnyAsync(x => x.Id == libroId);
            if(!libroExist)
            {
                return NotFound();
            }
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if(usuario is null)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.FechaCreacion = DateTime.UtcNow;
            comentario.UsuarioId = usuario.Id;

            context.Add(comentario);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);

            return CreatedAtRoute("ObtenerComentarioV2", new { id = comentario.Id, libroId }, comentarioDTO);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(Guid id, int libroId, JsonPatchDocument<ComentarioPatchDTO> pathcDoc)
        {
            if(pathcDoc is null) 
            {
                return BadRequest();
            }

            var libroExist = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!libroExist)
            {
                return NotFound();
            }

            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario is null)
            {
                return NotFound();
            }

            var usuario = await servicioUsuarios.ObtenerUsuario();
            if(usuario is null)
            {
                return NotFound();
            }

            
            if (usuario.Id != comentario.UsuarioId)
            {
                return Forbid();
            }

            var comentarioPatchDTO = mapper.Map<ComentarioPatchDTO>(comentario);
            pathcDoc.ApplyTo(comentarioPatchDTO, ModelState);
            var isValid = TryValidateModel(comentarioPatchDTO);
            if (!isValid)
            {
                return ValidationProblem();
            }

            mapper.Map(comentarioPatchDTO, comentario);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int libroId)
        {
            var libroExist = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!libroExist) { return NotFound(); }

            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario is null) { return NotFound();  }

            var usuario = await servicioUsuarios.
                ObtenerUsuario();
            if(usuario is null) { return NotFound(); }

            if(usuario.Id != comentario.UsuarioId)
            {
                return Forbid();
            }

            comentario.EstaBorrado = true;
            context.Update(comentario);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }
    }
}
