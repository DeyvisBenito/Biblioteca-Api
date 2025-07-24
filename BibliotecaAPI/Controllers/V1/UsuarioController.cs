using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs.UsuariosDTO;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/usuario")]
    public class UsuarioController: ControllerBase
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;

        public UsuarioController(UserManager<Usuario> userManager, IConfiguration configuration, 
                SignInManager<Usuario> signInManager, IServicioUsuarios servicioUsuarios, ApplicationDBContext context,
                IMapper mapper)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.servicioUsuarios = servicioUsuarios;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet(Name ="ObtenerUsuariosV1")]
        [Authorize(Policy ="EsAdmin")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> Get()
        {
            var usuarios = await context.Users.ToListAsync();
            var usuariosDTO = mapper.Map<List<UsuarioDTO>>(usuarios);

            return usuariosDTO;
        }

        [HttpPost("registro", Name ="InsertarUsuarioV1")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuario)
        {
            var usuario = new Usuario
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password!);

            if (resultado.Succeeded)
            {
                var respuesta = await CrearToken(credencialesUsuario);
                return respuesta;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem();
            }
        }

        [HttpPost("hacer-admin", Name ="HacerAdminV1")]
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimsDTO editarClaims)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaims.Email);
            if(usuario is null)
            {
                return NotFound();
            }

            await userManager.AddClaimAsync(usuario, new Claim("EsAdmin", "true"));
            return NoContent();
        }

        [HttpPost("remover-admin", Name ="RemoverAdminV1")]
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimsDTO editarClaims)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaims.Email);
            if(usuario is null)
            {
                return NotFound();
            }

            await userManager.RemoveClaimAsync(usuario, new Claim("EsAdmin", "true"));
            return NoContent();
        }

        [HttpPost("login", Name ="LoginV1")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuarioDTO credencialesUsuario)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            if(usuario is null)
            {
                return LoginIncorrecto();
            }
            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuario.Password!, 
                                    lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var token = await CrearToken(credencialesUsuario);
                return token;
            }
            else
            {
                return LoginIncorrecto();
            }
        }

        [HttpGet("renovar-token", Name ="RenovarTokenV1")]
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if(usuario is null)
            {
                return NotFound();
            }

            var credencialesUsuario = new CredencialesUsuarioDTO() { Email = usuario.Email! };
            var respuesta = await CrearToken(credencialesUsuario);

            return respuesta;
        }

        [HttpPut(Name ="ActualizarUsuarioV1")]
        [Authorize]
        public async Task<ActionResult> Put(ActualizarUsuarioDTO actualizarUsuario)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if(usuario is null)
            {
                return NotFound();
            }
            usuario.fechaNacimiento = actualizarUsuario.fechaNacimiento;
            await userManager.UpdateAsync(usuario);

            return NoContent();

        }

        //Crear token
        private async Task<RespuestaAutenticacionDTO> CrearToken(CredencialesUsuarioDTO credencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email)
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario!);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]!));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion,
                signingCredentials: credenciales);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenSeguridad);

            return new RespuestaAutenticacionDTO()
            {
                Token = token,
                Expiracion = expiracion
            };
        }

        //Login incorrecto
        private ActionResult LoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Login incorrecto");
            return ValidationProblem();
        }
    }
}
