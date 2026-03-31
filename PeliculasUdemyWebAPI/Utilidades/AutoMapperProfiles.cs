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
                .ForMember(destino => destino.Poster, opciones => opciones.Ignore()); // Ignorar el mapeo del campo Poster, ya que se maneja de forma separada
            CreateMap<Pelicula, PeliculaDTO>();
        }

        private void ConfigurarMapeosUsuario()
        {
            CreateMap<IdentityUser, UsuarioDTO>();
        }
    }
}
