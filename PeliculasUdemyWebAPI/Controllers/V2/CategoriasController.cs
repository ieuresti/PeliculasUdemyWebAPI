using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;
using PeliculasUdemyWebAPI.Servicios;

namespace PeliculasUdemyWebAPI.Controllers.V2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esAdmin")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository categoriaRepository;
        private readonly IImportacionExcelService importacionService;
        private readonly IMapper mapper;

        public CategoriasController(ICategoriaRepository categoriaRepository,
            IImportacionExcelService importacionService, IMapper mapper)
        {
            this.categoriaRepository = categoriaRepository;
            this.importacionService = importacionService;
            this.mapper = mapper;
        }

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

        [HttpPost("importar-excel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImportacionResultadoDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<ActionResult<ImportacionResultadoDTO>> ImportarDesdeExcel(IFormFile archivo)
        {
            // Validar que se envió un archivo
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("No se proporcionó ningún archivo");
            }

            // Validar extensión del archivo
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                    "Solo se permiten archivos Excel (.xlsx o .xls)");
            }

            // Validar tamaño del archivo (max 5 MB)
            if (archivo.Length > 5 * 1024 * 1024)
            {
                return BadRequest("El archivo no debe superar los 5 MB");
            }

            try
            {
                using var stream = archivo.OpenReadStream(); // Abrir el archivo como stream para procesarlo sin cargarlo completamente en memoria

                var resultado = await importacionService.ImportarDesdeExcelAsync<CategoriaCreacionDTO, Categoria>(
                    stream,
                    new[] { "Nombre" }, // Columnas esperadas
                    dto => mapper.Map<Categoria>(dto), // Convertir DTO a entidad
                    async categoria =>
                    {
                        if (await categoriaRepository.ExisteCategoriaAsync(categoria.Nombre))
                        {
                            return false; // Indicar que no se creó porque ya existe una categoría con ese nombre
                        }

                        return await categoriaRepository.CrearCategoriaAsync(categoria);
                    }
                );

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar el archivo: {ex.Message}");
            }
        }

        [HttpGet("plantilla-excel")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DescargarPlantillaExcel()
        {
            var columnas = new[] { "Nombre" };
            var datosEjemplo = new[]
            {
                new[] { "Acción" },
                new[] { "Comedia" },
                new[] { "Drama" }
            };

            var bytes = importacionService.GenerarPlantilla("Categorías", columnas, datosEjemplo);
            var fileName = $"PlantillaCategorias_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
