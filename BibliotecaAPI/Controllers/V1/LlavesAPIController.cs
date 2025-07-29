using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/llaves")]
    [Authorize]
    public class LlavesAPIController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IServicioLlaveAPI servicioLlaveAPI;

        public LlavesAPIController(ApplicationDBContext context, IMapper mapper,
                                    IServicioUsuarios servicioUsuarios, IServicioLlaveAPI servicioLlaveAPI)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioUsuarios = servicioUsuarios;
            this.servicioLlaveAPI = servicioLlaveAPI;
        }

        [HttpGet]
        public async Task<IEnumerable<LlaveAPIDTO>> Get()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var llaves = await context.LlavesAPI.Where(llave => llave.UsuarioId == usuarioId).ToListAsync();

            var llavesDTO = mapper.Map<IEnumerable<LlaveAPIDTO>>(llaves);

            return llavesDTO;
        }

        [HttpGet("{id:int}", Name = "ObtenerLlaveV1")]
        public async Task<ActionResult<LlaveAPIDTO>> Get(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if(usuarioId is null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no valido");
                return ValidationProblem();
            }

            var llave = await context.LlavesAPI.FirstOrDefaultAsync(llave => llave.Id == id);
            if(llave is null)
            {
                ModelState.AddModelError("Llave", "Llave no encontrada");
                return ValidationProblem();
            }

            if(llave.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            var llaveDTO = mapper.Map<LlaveAPIDTO>(llave);

            return Ok(llaveDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LlaveCreacionDTO llaveCreacionDTO)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if(usuarioId is null)
            {
                return ValidationProblem("Usuario no encontrado");
            }

            if(llaveCreacionDTO.TipoLlave == TipoLlave.Gratuita)
            {
                var usuarioTieneLlaveGratuita = await context.LlavesAPI.AnyAsync(x => x.TipoLlave == TipoLlave.Gratuita
                                                    && x.UsuarioId == usuarioId);

                if (usuarioTieneLlaveGratuita)
                {
                    ModelState.AddModelError(nameof(llaveCreacionDTO.TipoLlave), "El usuario ya tiene una llave gratuita");
                    return ValidationProblem();
                }
            }

            var llaveAPI = await servicioLlaveAPI.CrearLlaveAPI(usuarioId, llaveCreacionDTO.TipoLlave);
            var llaveAPIDTO = mapper.Map<LlaveAPIDTO>(llaveAPI);

            return CreatedAtRoute("ObtenerLlaveV1", new { id = llaveAPI.Id }, llaveAPIDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LlaveActualizacionDTO llaveActualizacionDTO)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if(usuarioId is null)
            {
                return ValidationProblem("Usuario sin sesion iniciado");
            }

            var llave = await context.LlavesAPI.FirstOrDefaultAsync(llave => llave.Id == id && llave.UsuarioId == usuarioId);

            if(llave is null)
            {
                return NotFound();
            }

            if(llave.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            if (llaveActualizacionDTO.ActualizarLlave)
            {
                llave.Llave = servicioLlaveAPI.GenerarLlave();
            }

            llave.Activa = llaveActualizacionDTO.Activa;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if(usuarioId is null)
            {
                return ValidationProblem("El usuario no tiene sesion");
            }
            if (llaveDB is null)
            {
                return NotFound();
            }
            if(llaveDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            if(llaveDB.TipoLlave == TipoLlave.Gratuita)
            {
                ModelState.AddModelError("", "No se puede eliminar una llave gratuita");
                return ValidationProblem();
            }

            context.Remove(llaveDB);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
