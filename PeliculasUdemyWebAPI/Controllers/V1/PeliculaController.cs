using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;

namespace PeliculasUdemyWebAPI.Controllers.V1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esAdmin")]
    public class PeliculaController : ControllerBase
    {
        private readonly IPeliculaRepository peliculaRepository;
        private readonly IMapper mapper;

        public PeliculaController(IPeliculaRepository peliculaRepository, IMapper mapper)
        {
            this.peliculaRepository = peliculaRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPeliculas()
        {
            var peliculas = await peliculaRepository.GetPeliculasAsync();
            var peliculasDTO = mapper.Map<ICollection<PeliculaDTO>>(peliculas);
            return Ok(peliculasDTO);
        }

        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPelicula(int peliculaId)
        {
            var pelicula = await peliculaRepository.GetPeliculaAsync(peliculaId);
            if (pelicula is null)
            {
                return NotFound();
            }
            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);
            return Ok(peliculaDTO);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreatePelicula([FromForm] PeliculaCreacionDTO peliculaCreacionDTO) // FromForm ya que se recibe un archivo a través de un formulario multipart/form-data
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (peliculaCreacionDTO is null)
            {
                return BadRequest(ModelState);
            }
            if (await peliculaRepository.ExistePeliculaAsync(peliculaCreacionDTO.Nombre))
            {
                ModelState.AddModelError("", "La película con ese nombre ya existe");
                return BadRequest(ModelState);
            }

            // TO DO - Guardar el archivo del poster en el servidor y obtener la ruta para almacenarla en la base de datos

            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);
            await peliculaRepository.CrearPeliculaAsync(pelicula);
            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);
            return CreatedAtRoute("GetPelicula", new { peliculaId = peliculaDTO.Id }, peliculaDTO);
        }

        [HttpPatch("{peliculaId:int}", Name = "UpdatePelicula")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdatePelicula(int peliculaId, [FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (peliculaCreacionDTO is null)
            {
                return BadRequest(ModelState);
            }
            if (!await peliculaRepository.ExistePeliculaAsync(peliculaId))
            {
                return NotFound();
            }

            // TO DO - Guardar el archivo del poster en el servidor y obtener la ruta para almacenarla en la base de datos

            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);
            pelicula.Id = peliculaId;
            await peliculaRepository.ActualizarPeliculaAsync(pelicula);
            return NoContent();
        }

        [HttpDelete("{peliculaId:int}", Name = "DeletePelicula")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeletePelicula(int peliculaId)
        {
            var existePelicula = await peliculaRepository.GetPeliculaAsync(peliculaId);
            if (existePelicula is null)
            {
                return NotFound($"No se encontró la película con ID {peliculaId}");
            }
            await peliculaRepository.BorrarPeliculaAsync(existePelicula);
            return NoContent();
        }

        [HttpGet("categoria/{categoriaId:int}", Name = "GetPeliculasEnCategoria")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPeliculasEnCategoria(int categoriaId)
        {
            var peliculasEnCategoria = await peliculaRepository.GetPeliculasEnCategoriaAsync(categoriaId);
            if (peliculasEnCategoria is null || !peliculasEnCategoria.Any())
            {
                return NotFound($"No se encontraron películas en la categoría con ID {categoriaId}");
            }
            var peliculasEnCategoriaDTO = mapper.Map<ICollection<PeliculaDTO>>(peliculasEnCategoria);
            return Ok(peliculasEnCategoriaDTO);
        }

        [HttpGet("buscar", Name = "BuscarPelicula")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> BuscarPelicula([FromQuery] string termino) // FromQuery ya que se recibe el texto a buscar como un parámetro de consulta en la URL
        {
            var peliculas = await peliculaRepository.BuscarPeliculaAsync(termino);
            if (peliculas is null || !peliculas.Any())
            {
                return NotFound($"No se encontraron películas con el texto: '{termino}'");
            }
            var peliculasDTO = mapper.Map<ICollection<PeliculaDTO>>(peliculas);
            return Ok(peliculasDTO);
        }
    }
}
