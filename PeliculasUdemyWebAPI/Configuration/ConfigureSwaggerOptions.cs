using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PeliculasUdemyWebAPI.Configuration
{
    // Esta clase se encarga de configurar las opciones de Swagger para cada versión de la API
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        // Este método se llama para configurar las opciones de SwaggerGen
        public void Configure(SwaggerGenOptions options)
        {
            // Crear un documento Swagger por cada versión de API descubierta
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    CreateInfoForApiVersion(description));
            }
        }

        // Método auxiliar para crear la información de OpenAPI para cada versión de API
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = $"API de Películas {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                ? "Esta versión de la API está obsoleta o deprecada y puede ser eliminada en el futuro."
                : "API REST para gestión de películas con autenticación JWT",
                Contact = new OpenApiContact
                {
                    Name = "Equipo de Desarrollo",
                    Email = "contacto@peliculas.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            };

            return info;
        }
    }
}
