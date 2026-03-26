using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PeliculasUdemyWebAPI.Configuration;
using PeliculasUdemyWebAPI.Data;
using PeliculasUdemyWebAPI.Repositorio;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configuracion para habilitar el caché
builder.Services.AddResponseCaching();

// Configuracion para habilitar la versionamiento de la API
var apiVersioningBuilder = builder.Services.AddApiVersioning(opciones =>
{
    opciones.DefaultApiVersion = new ApiVersion(1, 0);
    opciones.AssumeDefaultVersionWhenUnspecified = true;
    opciones.ReportApiVersions = true;
    opciones.ApiVersionReader = new UrlSegmentApiVersionReader(); // Esto es para que la version de la API se especifique en la URL (ejemplo: /api/v1/categorias)
});

apiVersioningBuilder.AddApiExplorer(opciones =>
{
    opciones.GroupNameFormat = "'v'VVV"; // Esto es para que el grupo de versiones se muestre en el formato "v1", "v2", etc. en la documentación de Swagger
    opciones.SubstituteApiVersionInUrl = true; // Esto es para que la versión de la API se sustituya en la URL de los endpoints en la documentación de Swagger
});

// Configuracion de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
// Registrar la clase de configuración de Swagger para que se ejecute al configurar las opciones de SwaggerGen
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(opciones =>
{
    // Definir esquema de seguridad JWT (Bearer Token)
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // El nombre del encabezado HTTP que se utilizará para enviar el token JWT (por lo general, "Authorization")
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Autenticación JWT usando el esquema Bearer.\n\n" +
            "Ingrese **solo** el token JWT en el campo de texto a continuación.\n\n" +
            "Ejemplo: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`\n\n" +
            "**No** es necesario agregar 'Bearer' al inicio, Swagger lo agrega automáticamente."
    });
    // Aplicar seguridad a todos los endpoints
    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>() // No se requieren scopes específicos para este esquema de seguridad, por lo que se pasa un array vacío
        }
    });
}
);

var origenesPermitidos = builder.Configuration.GetValue<string>("Cors:AllowedOrigins")!.Split(","); // Obtener los orígenes permitidos desde la configuración (appsettings.json) y dividirlos en un array utilizando la coma como separador

// Habilitar CORS
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.WithOrigins(origenesPermitidos)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Configuracion AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configuracion de inyección de dependencias para el repositorio
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPeliculaRepository, PeliculaRepository>();

builder.Services.AddHttpContextAccessor(); // Esto es para poder acceder al HttpContext en el repositorio (para obtener el usuario logueado, etc.)

// Configuracion DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opciones => 
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuracion de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
    // Configuración de contraseñas
    opciones.Password.RequireDigit = true;
    opciones.Password.RequireLowercase = true;
    opciones.Password.RequireUppercase = true;
    opciones.Password.RequireNonAlphanumeric = false;
    opciones.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Configuracion para utilizar Entity Framework Core como el almacenamiento de datos para Identity
.AddDefaultTokenProviders(); // Configuracion para utilizar los token providers por defecto de Identity (para generar tokens de confirmacion de email, restablecimiento de contraseña, etc.)

// Configuracion del servicio de autenticacion JWT
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false; // Esto es para evitar que no se cambien los nombres de los claims que ASP.NET Core cambia por defecto
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // No validamos el emisor del token
        ValidateAudience = false, // No validamos el destinatario del token
        ValidateLifetime = true, // Validamos la expiracion del token
        ValidateIssuerSigningKey = true, // Validamos la firma del token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)), // Configuracion de la clave de firma del token y el algoritmo de firma
        ClockSkew = TimeSpan.Zero // Esto es para evitar que se permita un margen de tiempo para la expiracion del token (por defecto es de 5 minutos)
    };
});

// Configuracion politicas de autorizacion
builder.Services.AddAuthorization(opciones =>
{
    // Esta política exige que el usuario autenticado tenga un claim cuyo tipo sea esAdmin. Si el claim no existe, la autorización falla (403)
    opciones.AddPolicy("esAdmin", politica => politica.RequireClaim("esAdmin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita el middleware de Swagger
    app.UseSwaggerUI(opciones =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Esto es para agregar un endpoint de Swagger por cada versión de API descubierta, mostrando el nombre del grupo de versiones en el título del endpoint (ejemplo: "API de Películas V1", "API de Películas V2", etc.)
        foreach (var description in provider.ApiVersionDescriptions)
        {
            opciones.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"API de Películas {description.GroupName.ToUpperInvariant()}"
            );
        }

        opciones.DefaultModelsExpandDepth(-1); // Ocultar modelos de los endpoints por defecto
        opciones.DocExpansion(DocExpansion.None); // Colapsar todos los endpoints por defecto
        opciones.DisplayRequestDuration(); // Mostrar tiempos de respuesta
        opciones.EnableDeepLinking(); // URLs directas a cada endpoint para facilitar compartir enlaces a endpoints especifícos
    }); // Habilita la interfaz de usuario de Swagger
}

app.UseHttpsRedirection();

app.UseCors();

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
