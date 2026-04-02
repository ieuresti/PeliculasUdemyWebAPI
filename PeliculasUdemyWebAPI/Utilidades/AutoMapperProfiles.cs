using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;

namespace PeliculasUdemyWebAPI.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            ConfigurarMapeosCategoria();
            ConfigurarMapeosActor();
            ConfigurarMapeosPelicula();
            ConfigurarMapeosUsuario();
        }

        private void ConfigurarMapeosCategoria()
        {
            CreateMap<CategoriaCreacionDTO, Categoria>();
            CreateMap<Categoria, CategoriaDTO>();
        }

        private void ConfigurarMapeosActor()
        {
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(destino => destino.Foto, opciones => opciones.Ignore()); // Ignorar el mapeo del campo Foto, ya que se maneja de forma separada
            CreateMap<Actor, ActorDTO>();
        }

        private void ConfigurarMapeosPelicula()
        {
            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(destino => destino.Poster, opciones => opciones.Ignore()) // Ignorar el mapeo del campo Poster, ya que se maneja de forma separada
                .ForMember(destino => destino.PeliculaCategorias, opciones => opciones.Ignore()) // Ignorar el mapeo de la propiedad de navegación a la tabla intermedia PeliculaCategoria, ya que se maneja de forma separada
                .ForMember(destino => destino.PeliculaActores, opciones => opciones.Ignore()); // Ignorar el mapeo de la propiedad de navegación a la tabla intermedia PeliculaActor, ya que se maneja de forma separada

            CreateMap<Pelicula, PeliculaDTO>()
                .ForMember(destino => destino.Categorias, opciones => 
                    opciones.MapFrom(origen => origen.PeliculaCategorias.Select(pc => pc.Categoria))) // Mapear la propiedad Categorias del DTO a partir de la relación con la tabla intermedia PeliculaCategoria
                .ForMember(destino => destino.Actores, opciones => 
                    opciones.MapFrom(origen => origen.PeliculaActores.Select(pa => pa.Actor))); // Mapear la propiedad Actores del DTO a partir de la relación con la tabla intermedia PeliculaActor
        }

        private void ConfigurarMapeosUsuario()
        {
            CreateMap<IdentityUser, UsuarioDTO>();
        }
    }
}
