using OfficeOpenXml;
using PeliculasUdemyWebAPI.Modelos;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PeliculasUdemyWebAPI.Servicios
{
    public interface ICategoriaImportacionService
    {
        Task<CategoriaImportacionResultadoDTO> ImportarDesdeExcelAsync(
            Stream archivoExcel,
            Func<Categoria, Task<bool>> guardarCategoriaFunc);
    }

    public class CategoriaImportacionService : ICategoriaImportacionService
    {
        public async Task<CategoriaImportacionResultadoDTO> ImportarDesdeExcelAsync(
            Stream archivoExcel,
            Func<Categoria, Task<bool>> guardarCategoriaFunc)
        {
            var resultado = new CategoriaImportacionResultadoDTO();

            using var package = new ExcelPackage(archivoExcel); // Cargar el archivo Excel
            var worksheet = package.Workbook.Worksheets[0]; // Obtener la primera hoja

            // Validar que tenga las columnas correctas
            if (worksheet.Cells[1, 1].Text != "Nombre")
            {
                resultado.Errores.Add(new ErrorImportacionDTO
                {
                    Fila = 1,
                    Campo = "Encabezado",
                    Mensaje = "La primera columna debe llamarse 'Nombre'"
                });
                return resultado;
            }

            var filaInicio = 2; // Asumimos que la fila 1 es el encabezado y los datos comienzan en la fila 2
            var filaFin = worksheet.Dimension?.End.Row ?? 1; // Obtener la última fila con datos

            resultado.TotalFilas = filaFin - filaInicio + 1; // Calcular el total de filas a procesar (excluyendo el encabezado)

            for (int fila = filaInicio; fila <= filaFin; fila++)
            {
                var nombre = worksheet.Cells[fila, 1].Text?.Trim();

                // Crear DTO para validación
                var categoriaDTO = new CategoriaCreacionDTO
                {
                    Nombre = nombre
                };

                // Validar con DataAnnotations
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(categoriaDTO);

                if (!Validator.TryValidateObject(categoriaDTO, validationContext, validationResults, true))
                {
                    foreach (var validationResult in validationResults)
                    {
                        resultado.Errores.Add(new ErrorImportacionDTO
                        {
                            Fila = fila,
                            Campo = validationResult.MemberNames.FirstOrDefault() ?? "Desconocido",
                            Mensaje = validationResult.ErrorMessage ?? "Error de validación"
                        });
                    }
                    resultado.CategoriasConErrores++;
                    continue;
                }

                // Crear entidad
                var categoria = new Categoria
                {
                    Nombre = nombre!,
                    FechaCreacion = DateTime.Now
                };

                try
                {
                    var guardado = await guardarCategoriaFunc(categoria); // Intentar guardar usando la función proporcionada
                    if (guardado)
                    {
                        resultado.CategoriasCreadas++; // Si se guardó correctamente, incrementar el contador
                    }
                    else
                    {
                        resultado.Errores.Add(new ErrorImportacionDTO
                        {
                            Fila = fila,
                            Campo = "Nombre",
                            Mensaje = "No se pudo guardar la categoría (posiblemente ya existe)"
                        });
                        resultado.CategoriasConErrores++;
                    }
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add(new ErrorImportacionDTO
                    {
                        Fila = fila,
                        Campo = "General",
                        Mensaje = $"Error al guardar: {ex.Message}"
                    });
                    resultado.CategoriasConErrores++;
                }
            }

            return resultado;
        }
    }
}
