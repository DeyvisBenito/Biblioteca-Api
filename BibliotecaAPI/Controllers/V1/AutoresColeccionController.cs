using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/autores-coleccion")]
    [Authorize(Policy = "EsAdmin")]
    public class AutoresColeccionController: ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;

        public AutoresColeccionController(ApplicationDBContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{ids}", Name ="ObtenerAutoresColV1")]
        [OutputCache]
        public async Task<ActionResult<List<AutorConLibrosDTO>>> Get(string ids)
        {
            var idsInt = new List<int>();

            foreach (var id in ids.Split(","))
            {
                if(int.TryParse(id, out int idInt))
                {
                    idsInt.Add(idInt);
                }
            }

            if (idsInt.Count == 0 || idsInt is null)
            {
                return NotFound();
            }

            var autores = await context.Autores
                                .Include(x => x.Libros)
                                      .ThenInclude(x => x.Libro)
                                .Where(x => idsInt.Contains(x.Id))
                                .ToListAsync();

            var autoresId = autores.Select(x => x.Id);
            if(idsInt.Count != autoresId.Count())
            {
                return NotFound();
            }

            var autoresDTO = mapper.Map<List<AutorConLibrosDTO>>(autores);
            return autoresDTO;
        }

        [HttpPost(Name = "InsertarAutoresV1")]
        public async Task<ActionResult> Post(IEnumerable<AutorCreacionDTO> autoresCreacionDTO)
        {
            var autores = mapper.Map<IEnumerable<Autor>>(autoresCreacionDTO);
            context.AddRange(autores);
            await context.SaveChangesAsync();
            var ids = autores.Select(x => x.Id);
            var idsString = string.Join(",", ids);

            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);

            return CreatedAtRoute("ObtenerAutoresV1", new { ids = idsString }, autoresDTO);
        }
    }
}
