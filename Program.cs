using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Inicio área de servicios

// Implementar CACHE
builder.Services.AddOutputCache(opciones =>
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
});

// Realizar excriptación en servicios
builder.Services.AddDataProtection();

var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;

// Habilitar CORS
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCors =>
    {
        opcionesCors.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader()
            .WithExposedHeaders("cantidad-total-registros");
    });
});

// Agregar automapper
builder.Services.AddAutoMapper(typeof(Program));

// Para trabajar con controladores con JSON PATCH
builder.Services.AddControllers(opciones =>
{   
    opciones.Conventions.Add(new ConvencionAgrupaPorVersion());

}).AddNewtonsoftJson();

// Para trabajar con controladores sin JSON PATCH
/*
builder.Services.AddControllers().AddJsonOptions(opciones => 
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
*/

// Agregar la conexión a la BD en SQL
builder.Services.AddDbContext<ApplicationDbContext>(opciones => 
    opciones.UseSqlServer("name=DefaultConnection"));

// Agregar el Identity
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Para registrar un usuario
builder.Services.AddScoped<UserManager<Usuario>>();

// Permite autenticar usuarios
builder.Services.AddScoped<SignInManager<Usuario>>();

// Implementación de un servicio aparte para administrar usuarios
builder.Services.AddTransient<IServiciosUsuarios, ServiciosUsuarios>();

// Implementación servicios archivos Azure
//builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();

// Implementación servicios archivos Local
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();

// Implementación de clase de filtros
builder.Services.AddScoped<MiFiltroDeAccion>();
builder.Services.AddScoped<FiltroValidacionLibro>();

// Utiliando una clase de version propia
builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IServicioAutores, BibliotecaAPI.Servicios.V1.ServicioAutores>();


// Filtros HATEOAS
builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IGeneradorEnlaces, BibliotecaAPI.Servicios.V1.GeneradorEnlaces>();
builder.Services.AddScoped<HATEOASAutorAttribute>();
builder.Services.AddScoped<HATEOASAutoresAttribute>();


// Permite acceder al contexto http desde cualquier clase
builder.Services.AddHttpContextAccessor();

// Configurar la autenticación
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)),
        ClockSkew = TimeSpan.Zero
    });


// Agregando politicas de seguridad como roles
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

// Instalación SWAGGER
builder.Services.AddSwaggerGen(opciones =>
{
    // Agregar descripción personalizada para swagger
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Biblioteca API -  Hola, Github Actions",
        Description = "Este es un API para trabajar con datos de autores y libros.",
        Contact = new OpenApiContact 
        {
           Email = "niko.d.roger@gmail.com",
           Name = "Nicolás Seguel",
           Url = new Uri("http://wwww.google.cl")           
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("http://opensource.org/license/mit/")
        }
    });

    opciones.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v2",
        Title = "Biblioteca API",
        Description = "Este es un API para trabajar con datos de autores y libros.",
        Contact = new OpenApiContact
        {
            Email = "niko.d.roger@gmail.com",
            Name = "Nicolás Seguel",
            Url = new Uri("http://wwww.google.cl")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("http://opensource.org/license/mit/")
        }
    });

    // Agregar configuración de seguridad tipo JWT a SWAGGER
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa: Bearer {tu_token}"
    });

    opciones.OperationFilter<FiltroAutoriacion>();

    // configuración JWT SWAGGER
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

// Fin área de servicios

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

    // Inicio área de Middlewares

    // Utilización de manejo de errores
    app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var excepcion = exceptionHandlerFeature?.Error!;

        var error = new Error()
        {
            MensajeDeError = excepcion.Message,
            StrackTrace = excepcion.StackTrace,
            Fecha = DateTime.UtcNow
        };

        var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
        dbContext.Add(error);
        await dbContext.SaveChangesAsync();
        await Results.InternalServerError(new
        {
            tipo = "error",
            mensaje = "Ha ocurrido un error inesperado.",
            estatus = 500
        }).ExecuteAsync(context);
    }));

// Usar SWAGGER
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API V1");
    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Biblioteca API V2");
});

app.UseAuthentication();
app.UseAuthorization();

// Para guardar archivos locales en wwwroot
app.UseStaticFiles();

// Usar CORS
app.UseCors();

// Usar el Cache
app.UseOutputCache();

// Usar controladores
app.MapControllers();

// Fin área de Middlewares


app.Run();


public partial class Program
{

}