using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/libros")]
    public class LibroController: ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly ITimeLimitedDataProtector protectorPorTiempo;
        private const string cache = "libros-obtenerV2";

        public LibroController(ApplicationDBContext context, IMapper mapper, IDataProtectionProvider protectionProvider, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            protectorPorTiempo = protectionProvider.CreateProtector("LibroController").ToTimeLimitedDataProtector();
        }



        [HttpGet]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<LibroDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryableLibro = context.Libros.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryableLibro);
            var libros = await queryableLibro.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            var librosDTO = mapper.Map<IEnumerable<LibroDTO>> (libros);

            return librosDTO;
        }

        [HttpGet("obtenenerToken")]
        [Authorize(Policy ="EsAdmin")]
        public ActionResult ObtenerTokenPorTiempo()
        {
            var token = Guid.NewGuid().ToString();
            var tokenAutorizado = protectorPorTiempo.Protect(token, lifetime: TimeSpan.FromSeconds(45));
            var url = Url.RouteUrl("obtenerLibrosAccesoTokenV2", new { tokenAutorizado }, "http");

            return Ok(url);
        }

        [HttpGet("obtenerLibrosToken/{tokenAutorizado}", Name = "obtenerLibrosAccesoTokenV2")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult> ObtenerLibrosAccesoToken(string tokenAutorizado)
        {
            try
            {
                protectorPorTiempo.Unprotect(tokenAutorizado);
            }
            catch
            {
                ModelState.AddModelError(nameof(tokenAutorizado), "Acceso expirado.");
                return ValidationProblem();
            }

            var libros = await context.Libros.ToListAsync();
            var librosDTO = mapper.Map<IEnumerable<LibroDTO>>(libros);
            return Ok(librosDTO);
        }


        [HttpGet("{Id:int}", Name = "ObtenerLibroV2")]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<LibroConAutorDTO>> Get(int Id)
        {
            var libro = await context.Libros
                .Include(x => x.Autores)
                        .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == Id);
            if(libro is null)
            {
                ModelState.AddModelError(nameof(libro.Id), $"El libro con Id {Id} no existe");
                return NotFound(new ValidationProblemDetails(ModelState));
            }

            var libroConAutorDTO = mapper.Map<LibroConAutorDTO>(libro);

            return Ok(libroConAutorDTO);
        }

        [HttpPost]
        [Authorize(Policy = "EsAdmin")]
        [ServiceFilter<FiltroValidacionLibro>()]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            var libro = mapper.Map<Libro>(libroCreacionDTO);
            EstablecerOrdenAutor(libro);
            context.Libros.Add(libro);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("ObtenerLibroV2", new {libro.Id}, libroDTO);
        }

        
        [HttpPut("{id:int}")]
        [Authorize(Policy = "EsAdmin")]
        [ServiceFilter<FiltroValidacionLibro>()]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroBD = await context.Libros.Include(x => x.Autores).FirstOrDefaultAsync(x => x.Id == id);
            if(libroBD is null) {
                ModelState.AddModelError(nameof(id), $"El libro con Id {id} no existe");

                return NotFound(new ValidationProblemDetails(ModelState)); 
            }

            mapper.Map(libroCreacionDTO, libroBD);
            EstablecerOrdenAutor(libroBD);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult> Delete(int id)
        {
            var librosEliminados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();
            if(librosEliminados == 0)
            {
                return NotFound("El registro a eliminar no ha sido encontrado");
            }
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }


        private void EstablecerOrdenAutor(Libro libro)
        {
            if(libro is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }
        }
    }
}
