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
        }

        private void ConfigurarMapeosCategoria()
        {
            CreateMap<CategoriaCreacionDTO, Categoria>();
            CreateMap<Categoria, CategoriaDTO>();
        }
    }
}
