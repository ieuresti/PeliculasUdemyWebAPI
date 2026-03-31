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
    public class ActorController : ControllerBase
    {
        private readonly IActorRepository actorRepository;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly IMapper mapper;
        private readonly string contenedor = "actores"; // Nombre del contenedor/carpeta donde se almacenarán los archivos de actores

        public ActorController(IActorRepository actorRepository, IAlmacenadorArchivos almacenadorArchivos, IMapper mapper)
        {
            this.actorRepository = actorRepository;
            this.almacenadorArchivos = almacenadorArchivos;
            this.mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetActores([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var totalActores = actorRepository.GetTotalActores();
            var actores = await actorRepository.GetActoresAsync(pageNumber, pageSize);

            if (actores is null || !actores.Any())
            {
                return NotFound("No se encontraron actores");
            }

            var actoresDTO = mapper.Map<ICollection<ActorDTO>>(actores);

            var response = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalActores / pageSize),
                TotalItems = totalActores,
                Items = actoresDTO
            };

            return Ok(response);
        }

        [HttpGet("{actorId:int}", Name = "GetActor")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ActorDTO>> GetActor(int actorId)
        {
            var actor = await actorRepository.GetActorAsync(actorId);
            if (actor is null)
            {
                return NotFound("Actor no encontrado");
            }
            var actorDTO = mapper.Map<ActorDTO>(actor);
            return Ok(actorDTO);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CrearActor([FromForm] ActorCreacionDTO actorCreacionDTO) // FromForm ya que se recibe un archivo a través de un formulario multipart/form-data
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (actorCreacionDTO is null)
            {
                return BadRequest(ModelState);
            }
            if (await actorRepository.ExisteActorAsync(actorCreacionDTO.Nombre))
            {
                ModelState.AddModelError("", "Ya existe un actor con ese nombre");
                return BadRequest(ModelState);
            }

            var actor = mapper.Map<Actor>(actorCreacionDTO); // Mapeamos el DTO a la entidad Actor
            if (actorCreacionDTO.Foto is not null)
            {
                // Si se ha enviado un archivo de imagen, lo almacenamos y obtenemos la URL para guardarla en la base de datos
                var urlFoto = await almacenadorArchivos.Almacenar(contenedor, actorCreacionDTO.Foto);
                actor.Foto = urlFoto;
            }

            var resultado = await actorRepository.CrearActorAsync(actor);
            if (!resultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el actor");
            }
            var actorDTO = mapper.Map<ActorDTO>(actor);
            return CreatedAtRoute("GetActor", new { actorId = actorDTO.Id }, actorDTO);
        }

        [HttpPatch("{actorId:int}", Name = "UpdateActor")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateActor(int actorId, [FromForm] ActorCreacionDTO actorCreacionDTO) // FromForm ya que se recibe un archivo a través de un formulario multipart/form-data
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (actorCreacionDTO is null)
            {
                return BadRequest(ModelState);
            }
            var actor = await actorRepository.GetActorAsync(actorId);
            if (actor is null)
            {
                return NotFound();
            }

            mapper.Map(actorCreacionDTO, actor); // Mapeamos el DTO de actualización sobre la entidad existente
            if (actorCreacionDTO.Foto is not null)
            {
                // Si se ha enviado un nuevo archivo de poster, lo almacenamos y obtenemos la URL para actualizarla en la base de datos. Además, eliminamos el archivo anterior si existía.
                var urlFoto = await almacenadorArchivos.Editar(actor.Foto, contenedor, actorCreacionDTO.Foto);
                actor.Foto = urlFoto;
            }

            var resultado = await actorRepository.ActualizarActorAsync(actor);
            if (!resultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al actualizar el actor");
            }
            return NoContent();
        }

        [HttpDelete("{actorId:int}", Name = "DeleteActor")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteActor(int actorId)
        {
            var existeActor = await actorRepository.GetActorAsync(actorId);
            if (existeActor is null)
            {
                return NotFound($"No se encontró el actor con ID {actorId}");
            }

            var resultado = await actorRepository.BorrarActorAsync(existeActor);
            if (!resultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al borrar el actor");
            }
            return NoContent();
        }
    }
}
