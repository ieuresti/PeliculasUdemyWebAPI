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
        private readonly ICategoriaImportacionService importacionService;
        private readonly IMapper mapper;

        public CategoriasController(ICategoriaRepository categoriaRepository,
            ICategoriaImportacionService importacionService, IMapper mapper)
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaImportacionResultadoDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<ActionResult<CategoriaImportacionResultadoDTO>> ImportarDesdeExcel(IFormFile archivo)
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

                // Función para guardar categoría
                async Task<bool> GuardarCategoria(Categoria categoria)
                {
                    // Verificar si ya existe
                    if (await categoriaRepository.ExisteCategoriaAsync(categoria.Nombre))
                    {
                        return false;
                    }

                    return await categoriaRepository.CrearCategoriaAsync(categoria);
                }

                var resultado = await importacionService.ImportarDesdeExcelAsync(stream, GuardarCategoria);

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
            using var package = new OfficeOpenXml.ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Categorías");

            // Encabezados
            worksheet.Cells[1, 1].Value = "Nombre";
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            // Ejemplos
            worksheet.Cells[2, 1].Value = "Acción";
            worksheet.Cells[3, 1].Value = "Comedia";
            worksheet.Cells[4, 1].Value = "Drama";

            // Ajustar ancho de columnas
            worksheet.Cells.AutoFitColumns();

            // Guardar el paquete en un MemoryStream para devolverlo como archivo descargable
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0; // Reiniciar la posición del stream al inicio para que se pueda leer correctamente al descargar el archivo

            var fileName = $"PlantillaCategorias_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
