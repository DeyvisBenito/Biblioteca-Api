using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs.UsuariosDTO;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BibliotecaAPITests.Utilidades
{
    //Se estara usando bases de datos en memoria para poder hacer pruebas con EFC ya que no se agrego una interfaz de esta
    //Se agraga la utilidad de AutoMapper para hacer las pruebas mas facilmente con los perfiles creados en utilidades
    public class BasePruebasDBContextIMapper
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected readonly Claim AdminClaim = new Claim("EsAdmin", "true");

        protected ApplicationDBContext ConstruirContext(string nombreBD)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDBContext>().UseInMemoryDatabase(nombreBD).Options;

            var dbContext = new ApplicationDBContext(opciones);
            return dbContext;
        }

        protected IMapper ConstruirMapper()
        {
            var config = new MapperConfiguration(opciones =>
            {
                opciones.AddProfile(new AutoMapperProfile());
            });

            return config.CreateMapper();
        }

        protected WebApplicationFactory<Program> ConstruirWebApplicationFactory(string nombreBD, bool ignorarSeguridad = true)
        {
            var factory = new WebApplicationFactory<Program>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor descriptorDBContext = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDBContext>))!;

                    if (descriptorDBContext is not null)
                    {
                        services.Remove(descriptorDBContext);
                    }
                    services.AddDbContext<ApplicationDBContext>(options =>
                    {
                        options.UseInMemoryDatabase(nombreBD);
                    });

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymusHandler>();
                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new UsuarioFalsoFiltro());
                        });
                    }
                });
            });

            return factory;
        }



        //Metodo auxiliar para CrearUsuario sin claims y sin email
        protected async Task<string> CrearUsuario(string db, WebApplicationFactory<Program> factory)
            => await CrearUsuario(db, factory, [], "ejemplo@gmail.com");

        //Metodo auxiliar para CrearUsuario sin email
        protected async Task<string> CrearUsuario(string db, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims)
            => await CrearUsuario(db, factory, claims, "ejemplo@gmail.com");

        protected async Task<string> CrearUsuario(string db, WebApplicationFactory<Program> factory,
            IEnumerable<Claim> claims, string email)
        {
            var urlRegistro = "api/v1/usuario/registro";
            string token = string.Empty;

            token = await ObtenerToken(email, urlRegistro, factory);

            if (claims.Any())
            {
                var context = ConstruirContext(db);
                var usuario = await context.Users.FirstOrDefaultAsync(x => x.Email == email);

                Assert.IsNotNull(usuario);

                var claimsUsuario = claims.Select(x => new IdentityUserClaim<string>
                {
                    UserId = usuario.Id,
                    ClaimType = x.Type,
                    ClaimValue = x.Value
                });

                context.UserClaims.AddRange(claimsUsuario);
                await context.SaveChangesAsync();

                var urlLogin = "api/v1/usuario/login";
                token = await ObtenerToken(email, urlLogin, factory);
            }

            return token;
        }

        private async Task<string> ObtenerToken(string email, string url, WebApplicationFactory<Program> factory)
        {
            var password = "aA123456!";
            var credenciales = new CredencialesUsuarioDTO
            {
                Email = email,
                Password = password
            };

            var cliente = factory.CreateClient();
            var respuesta = await cliente.PostAsJsonAsync(url, credenciales);

            respuesta.EnsureSuccessStatusCode();
            var resultado = await respuesta.Content.ReadAsStringAsync();
            var resultadoDeserializado = JsonSerializer.Deserialize<RespuestaAutenticacionDTO>(
                    resultado, jsonSerializerOptions
                );

            Assert.IsNotNull(resultadoDeserializado);

            return resultadoDeserializado.Token;
        }
    }
}
