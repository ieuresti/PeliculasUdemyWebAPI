using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PeliculasUdemyWebAPI.Data;
using PeliculasUdemyWebAPI.Repositorio;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configuracion de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // Configuracion para utilizar Entity Framework con Identity
    .AddDefaultTokenProviders(); // Configuracion para utilizar los token providers por defecto de Identity (para generar tokens de confirmacion de email, restablecimiento de contraseña, etc.)

builder.Services.AddScoped<UserManager<IdentityUser>>(); // Nos va a permitir gestionar los usuarios (crear, eliminar, actualizar, etc.)
builder.Services.AddScoped<SignInManager<IdentityUser>>(); // Nos va a permitir gestionar el inicio de sesión de los usuarios (iniciar sesión, cerrar sesión, etc.)

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
    app.UseSwaggerUI(); // Habilita la interfaz de usuario de Swagger
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
