using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Interfaces;
using BibliotecaAPI.Middlewares;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Area de servicios

//Agregando OutputCache
builder.Services.AddOutputCache(opciones =>
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

//OutputCache con Redis
//builder.Services.AddStackExchangeRedisOutputCache(opciones =>
//{
//    opciones.Configuration = builder.Configuration.GetConnectionString("redis");
//});


//Agregando el servicio de encriptado
builder.Services.AddDataProtection();

builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCors =>
    {
        opcionesCors.WithOrigins(builder.Configuration.GetSection("paginasPermitidas").Get<string[]>()!)
        .AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("cantidadTotalRegistros");
    });
});

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers( opciones =>
                                {
                                    opciones.Filters.Add<TiempoDeEjecucionFiltro>();
                                    opciones.Conventions.Add(new AgruparControllersConvencions());
                                }
       
                ).AddJsonOptions(option => option.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
                .AddNewtonsoftJson();


builder.Services.AddDbContext<ApplicationDBContext>(opciones => opciones.UseSqlServer("name=DefaultConection"));

//Configurando identity
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

//Configurando para agregar usuarios
builder.Services.AddScoped<UserManager<Usuario>>();
//Configurando para loguear
builder.Services.AddScoped<SignInManager<Usuario>>();

//Configurando para acceder a HTTP desde cualquier clase
builder.Services.AddHttpContextAccessor();

//Configurando Autenticacion, aqui el token que viene se valida por aqui
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false; //Para que no ASP.net no cambie los valores que ingreso al claim

        //Parametros de validacion del token
        opciones.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false, //Para que no valide el emisor del token
            ValidateAudience = false, //Para que no valide el receptor o audiencia del token
            ValidateLifetime = true, //Valida la expiracion del token
            ValidateIssuerSigningKey = true, //Valida la firma del token aqui es lo que trae "llavejwt"
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)), //Configurar la llave a comparar
            ClockSkew = TimeSpan.Zero //Para expirar el token en su tiempo debido sin variaciones
        };
    });

//Autenticacion por claim
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("EsAdmin"));
});

//Agregando servicio de usuarios
builder.Services.AddScoped<IServicioUsuarios, ServicioUsuarios>();

builder.Services.AddScoped<IServicioLlaveAPI, ServicioLlaveAPI>();


builder.Services.AddTransient<IAlmacenarArchivos, AlmacenarArchivosLocal>();
//Agregar una servicio en una version
builder.Services.AddScoped<BibliotecaAPI.Interfaces.IGenerarEnlaces, BibliotecaAPI.Servicios.V1.GenerarEnlaces>();

//Aggregando servicio de hash
builder.Services.AddTransient<IServicioHash, ServicioHash>();

//Servicio de filtros
builder.Services.AddScoped<PrimerFiltroPrueba>();
//Filtro de validacion de libro
builder.Services.AddScoped<FiltroValidacionLibro>();
//filtro HATEOAS para autores
builder.Services.AddScoped<HateoasAutorAttribute>();
builder.Services.AddScoped<HateoasAutoresAttribute>();

//Agregando Swagger y metiendo el jwt a swagger
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Biblioteca API",
        Description = "API de gestion de una biblioteca",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Email = "deyvisbenito@gmail.com",
            Name = "Deyvis Benito",
            Url = new Uri("https://deyvisbenito.github.io/Portafolio-Deyvis/")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/mit")
        }
    });

    opciones.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v2",
        Title = "Biblioteca API",
        Description = "API de gestion de una biblioteca",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Email = "deyvisbenito@gmail.com",
            Name = "Deyvis Benito",
            Url = new Uri("https://deyvisbenito.github.io/Portafolio-Deyvis/")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/mit")
        }
    });

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    opciones.OperationFilter <FilterAuthorization> ();

    //opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "Bearer"
    //            }
    //        },
    //        new string[]{}
    //    }
    //});
});

var app = builder.Build();

//Area de Middlewares

//Middleware de excepciones o errores
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error!;

    var error = new Error
    {
        MensajeDeError = exception.Message,
        StrackTrace = exception.StackTrace,
        Fecha = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDBContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();

    await Results.InternalServerError(new
    {
        tipo = "Error",
        Mensaje = "Ha ocurrido un error en el servidor",
        status = 500
    }).ExecuteAsync(context);

}));

app.UseSwagger();
app.UseSwaggerUI( opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API v1");
    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Biblioteca API v2");
});

app.UseCabeceraPersonalizada();
app.UseStaticFiles();
app.UseCors();

app.UseOutputCache();

app.UseLogueoPeticion();

//bloqueo
app.UseBloqueo();

app.MapControllers();

app.Run();

public partial class Program { }
