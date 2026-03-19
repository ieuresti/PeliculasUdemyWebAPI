using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;

namespace PeliculasUdemyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository categoriaRepository;
        private readonly IMapper mapper;

        public CategoriasController(ICategoriaRepository categoriaRepository, IMapper mapper)
        {
            this.categoriaRepository = categoriaRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCategorias()
        {
            var categorias = await categoriaRepository.GetCategoriasAsync();
            var categoriasDTO = mapper.Map<ICollection<CategoriaDTO>>(categorias);
            return Ok(categoriasDTO);
        }

        [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCategoria(int categoriaId)
        {
            var categoria = await categoriaRepository.GetCategoriaAsync(categoriaId);
            if (categoria is null)
            {
                return NotFound();
            }
            var categoriaDTO = mapper.Map<CategoriaDTO>(categoria);
            return Ok(categoriaDTO);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateCategoria([FromBody] CategoriaCreacionDTO categoriaCreacionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (categoriaCreacionDTO is null)
            {
                return BadRequest(ModelState);
            }
            if (await categoriaRepository.ExisteCategoriaAsync(categoriaCreacionDTO.Nombre))
            {
                ModelState.AddModelError("", "La categoría con ese nombre ya existe");
                return BadRequest(ModelState);
            }

            var categoria = mapper.Map<Categoria>(categoriaCreacionDTO);
            await categoriaRepository.CrearCategoriaAsync(categoria);
            var categoriaDTO = mapper.Map<CategoriaDTO>(categoria);
            return CreatedAtRoute("GetCategoria", new { categoriaId = categoriaDTO.Id }, categoriaDTO);
        }

        [HttpPatch("{categoriaId:int}", Name = "UpdatePatchCategoria")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdatePatchCategoria(int categoriaId, [FromBody] CategoriaCreacionDTO categoriaCreacionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existeCategoria = await categoriaRepository.GetCategoriaAsync(categoriaId);
            if (existeCategoria is null)
            {
                return NotFound($"No se encontró la categoría con ID {categoriaId}");
            }

            mapper.Map(categoriaCreacionDTO, existeCategoria);
            await categoriaRepository.ActualizarCategoriaAsync(existeCategoria);
            return NoContent();
        }

        [HttpPut("{categoriaId:int}", Name = "UpdatePutCategoria")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdatePutCategoria(int categoriaId, [FromBody] CategoriaCreacionDTO categoriaCreacionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existeCategoria = await categoriaRepository.GetCategoriaAsync(categoriaId);
            if (existeCategoria is null)
            {
                return NotFound($"No se encontró la categoría con ID {categoriaId}");
            }

            mapper.Map(categoriaCreacionDTO, existeCategoria); // Mapea los campos de categoriaCreacionDTO a existeCategoria, actualizando solo los campos que se proporcionan en el DTO
            await categoriaRepository.ActualizarCategoriaAsync(existeCategoria);
            return NoContent();
        }

        [HttpDelete("{categoriaId:int}", Name = "DeleteCategoria")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteCategoria(int categoriaId)
        {
            var existeCategoria = await categoriaRepository.GetCategoriaAsync(categoriaId);
            if (existeCategoria is null)
            {
                return NotFound($"No se encontró la categoría con ID {categoriaId}");
            }

            await categoriaRepository.BorrarCategoriaAsync(existeCategoria);
            return NoContent();
        }
    }
}
