using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PeliculasUdemyWebAPI.Controllers.V2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esAdmin")]
    public class CategoriasController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetCategorias()
        {
            var categorias = new List<string>
            {
                "Acción",
                "Comedia",
                "Drama",
                "Terror",
                "Ciencia ficción"
            };
            return Ok(categorias);
        }
    }
}
