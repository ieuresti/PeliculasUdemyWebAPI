using AutoMapper;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;

namespace PeliculasUdemyWebAPI.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            ConfigurarMapeosCategoria();
            ConfigurarMapeosPelicula();
        }

        private void ConfigurarMapeosCategoria()
        {
            CreateMap<CategoriaCreacionDTO, Categoria>();
            CreateMap<Categoria, CategoriaDTO>();
        }

        private void ConfigurarMapeosPelicula()
        {
            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(destino => destino.Poster, opciones => opciones.Ignore()); // Ignorar el mapeo del campo Poster, ya que se maneja de forma separada
            CreateMap<Pelicula, PeliculaDTO>();
        }
    }
}
