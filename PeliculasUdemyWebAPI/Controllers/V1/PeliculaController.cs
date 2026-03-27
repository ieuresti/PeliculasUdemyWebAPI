using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using PeliculasUdemyWebAPI.Repositorio.IRepositorio;
using PeliculasUdemyWebAPI.Servicios;

namespace PeliculasUdemyWebAPI.Controllers.V1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esAdmin")]
    public class PeliculaController : ControllerBase
    {
        private readonly IPeliculaRepository peliculaRepository;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly IMapper mapper;
        private readonly string contenedor = "peliculas"; // Nombre del contenedor/carpeta donde se almacenarán los archivos de películas

        public PeliculaController(IPeliculaRepository peliculaRepository, IAlmacenadorArchivos almacenadorArchivos, IMapper mapper)
        {
            this.peliculaRepository = peliculaRepository;
            this.almacenadorArchivos = almacenadorArchivos;
            this.mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPeliculas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var totalPeliculas = peliculaRepository.GetTotalPeliculas();
            var peliculas = await peliculaRepository.GetPeliculasAsync(pageNumber, pageSize);

            if (peliculas is null || !peliculas.Any())
            {
                return NotFound("No se encontraron películas");
            }

            var peliculasDTO = mapper.Map<ICollection<PeliculaDTO>>(peliculas);

            var response = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalPeliculas / pageSize),
                TotalItems = totalPeliculas,
                Items = peliculasDTO
            };

            return Ok(response);
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

            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO); // Mapeamos el DTO a la entidad Pelicula
            if (peliculaCreacionDTO.Poster is not null)
            {
                // Si se ha enviado un archivo de poster, lo almacenamos y obtenemos la URL para guardarla en la base de datos
                var url = await almacenadorArchivos.Almacenar(contenedor, peliculaCreacionDTO.Poster);
                pelicula.Poster = url;
            }

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

            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);
            if (peliculaCreacionDTO.Poster is not null)
            {
                // Si se ha enviado un nuevo archivo de poster, lo almacenamos y obtenemos la URL para actualizarla en la base de datos. Además, eliminamos el archivo anterior si existía.
                pelicula.Poster = await almacenadorArchivos.Editar(pelicula.Poster, contenedor, peliculaCreacionDTO.Poster);
            }
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
