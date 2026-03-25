using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PeliculasUdemyWebAPI.Data;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PeliculasUdemyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esAdmin")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public UsuariosController(UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager, IConfiguration configuration, 
            ApplicationDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UsuarioDTO>>> GetUsuarios()
        {
            var queryable = context.Users.AsQueryable();
            var usuarios = await queryable
                .ProjectTo<UsuarioDTO>(mapper.ConfigurationProvider)
                .OrderBy(x => x.Email)
                .ToListAsync();
            return usuarios;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CreadencialesUsuarioDTO credencialesUsuarioDTO)
        {
            // Verificar si ya existe un usuario registrado con el mismo email
            var existeUsuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            if (existeUsuario is not null)
            {
                var errores = new List<IdentityError>
                {
                    new IdentityError
                    {
                        Code = "UsuarioExistente",
                        Description = "Ya existe un usuario registrado con ese email"
                    }
                };
                return BadRequest(errores);
            }

            // Crear un nuevo usuario con el email y password proporcionados
            var usuario = new IdentityUser
            {
                Email = credencialesUsuarioDTO.Email,
                UserName = credencialesUsuarioDTO.Email
            };

            // Intentar crear el usuario en la base de datos
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password);

            if (resultado.Succeeded)
            {
                // Si el usuario se creó correctamente, construir y retornar el token de autenticación
                return await ConstruirToken(usuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CreadencialesUsuarioDTO credencialesUsuarioDTO)
        {
            // Buscar el usuario en la base de datos por su email
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);

            if (usuario is null)
            {
                // Si no se encuentra el usuario, construir y retornar un error de login incorrecto
                var errores = ConstruirLoginIncorrecto();
                return BadRequest(errores);
            }

            // Verificar que la contraseña proporcionada sea correcta para el usuario encontrado
            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuarioDTO.Password, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                // Si el login fue exitoso, construir y retornar el token de autenticación
                return await ConstruirToken(usuario);
            }
            else
            {
                var errores = ConstruirLoginIncorrecto();
                return BadRequest(errores);
            }
        }

        [HttpPost("hacerAdmin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null)
            {
                return NotFound();
            }
            // Agregar el claim "esAdmin" al usuario para otorgarle privilegios de administrador
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }

        [HttpPost("removerAdmin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null)
            {
                return NotFound();
            }
            // Eliminar el claim "esAdmin" del usuario para revocar sus privilegios de administrador
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(IdentityUser identityUser)
        {
            // Agregar los claims que se quieran incluir en el token
            var claims = new List<Claim>
            {
                new Claim("id", identityUser.Id),
                new Claim("email", identityUser.Email!),
                new Claim("username", identityUser.UserName!),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), // Issued At (cúando se emitió el token)
            };

            // Agregar roles del usuario
            var roles = await userManager.GetRolesAsync(identityUser);
            claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

            // Obtener los claims personalizados que se hayan agregado al usuario en la base de datos
            var claimsDB = await userManager.GetClaimsAsync(identityUser);
            // Agregar los claims personalizados al token
            claims.AddRange(claimsDB);

            // Crear la llave y credenciales que se van a usar para firmar el token
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]!));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            // Definir la expiración del token
            var expiracion = DateTime.UtcNow.AddHours(1);

            // Crear el token de seguridad
            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: credenciales);

            // Escribir el token de seguridad en formato string
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            // Retornar el token y la fecha de expiración
            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }

        private IEnumerable<IdentityError> ConstruirLoginIncorrecto()
        {
            // Construir un error de identidad personalizado para indicar que el login fue incorrecto
            var identityError = new IdentityError
            {
                Code = "LoginIncorrecto",
                Description = "Usuario o contraseña incorrectos"
            };
            // Retornar una lista con el error de identidad personalizado
            var errores = new List<IdentityError>();
            errores.Add(identityError);
            return errores;
        }
    }
}
