using AutoMapper;
using OfficeOpenXml;
using PeliculasUdemyWebAPI.Modelos.DTOs;
using System.ComponentModel.DataAnnotations;

// Servicio genérico para importar datos desde un archivo Excel a cualquier entidad del sistema
namespace PeliculasUdemyWebAPI.Servicios
{
    public interface IImportacionExcelService
    {
        Task<ImportacionResultadoDTO> ImportarDesdeExcelAsync<TDto, TEntity>(
            Stream archivoExcel,
            string[] columnasEsperadas,
            Func<TDto, TEntity> convertirDtoAEntidad,
            Func<TEntity, Task<bool>> guardarEntidadFunc)
            where TDto : class, new()
            where TEntity : class;

        byte[] GenerarPlantilla(string nombreHoja, string[] columnas, string[][]? datosEjemplo = null);
    }

    public class ImportacionExcelService : IImportacionExcelService
    {
        private readonly IMapper _mapper;

        public ImportacionExcelService(IMapper mapper)
        {
            _mapper = mapper;
        }

        // Método genérico para importar datos desde Excel, validando con DataAnnotations y reportando errores detallados
        public async Task<ImportacionResultadoDTO> ImportarDesdeExcelAsync<TDto, TEntity>(
            Stream archivoExcel,
            string[] columnasEsperadas,
            Func<TDto, TEntity> convertirDtoAEntidad,
            Func<TEntity, Task<bool>> guardarEntidadFunc)
            where TDto : class, new()
            where TEntity : class
        {
            var resultado = new ImportacionResultadoDTO();

            using var package = new ExcelPackage(archivoExcel); // Cargar el archivo Excel
            var worksheet = package.Workbook.Worksheets[0]; // Obtener la primera hoja de trabajo

            // Validar que las columnas en el Excel coincidan con las columnas esperadas
            for (int i = 0; i < columnasEsperadas.Length; i++)
            {
                var nombreColumna = worksheet.Cells[1, i + 1].Text?.Trim();
                if (nombreColumna != columnasEsperadas[i])
                {
                    // Si el nombre de la columna no coincide, agregar un error al resultado
                    resultado.Errores.Add(new ErrorImportacionDTO
                    {
                        Fila = 1,
                        Campo = $"Columna {i + 1}",
                        Mensaje = $"Se esperaba '{columnasEsperadas[i]}' pero se encontró '{nombreColumna}'"
                    });
                }
            }

            // Si hay errores en los encabezados, no continuar con la importación
            if (resultado.Errores.Any())
            {
                return resultado;
            }

            var filaInicio = 2; // Asumimos que la primera fila es de encabezados
            var filaFin = worksheet.Dimension?.End.Row ?? 1; // Obtener la última fila con datos en la hoja de trabajo
            resultado.TotalFilas = filaFin - filaInicio + 1; // Calcular el total de filas a procesar

            // Iterar sobre cada fila del Excel, mapear a DTO, validar y guardar
            for (int fila = filaInicio; fila <= filaFin; fila++)
            {
                try
                {
                    var dto = MapearFilaADto<TDto>(worksheet, fila, columnasEsperadas);

                    // Validar el DTO utilizando DataAnnotations
                    var validationResults = new List<ValidationResult>();
                    var validationContext = new ValidationContext(dto);

                    // Si la validación falla, agregar los errores al resultado y continuar con la siguiente fila
                    if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
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
                        resultado.RegistrosConErrores++;
                        continue;
                    }

                    // Convertir DTO a entidad
                    var entidad = convertirDtoAEntidad(dto);

                    // Intentar guardar la entidad utilizando la función proporcionada y actualizar el resultado según el éxito o fracaso de la operación
                    var guardado = await guardarEntidadFunc(entidad);
                    if (guardado)
                    {
                        resultado.RegistrosCreados++;
                    }
                    else
                    {
                        resultado.Errores.Add(new ErrorImportacionDTO
                        {
                            Fila = fila,
                            Campo = "General",
                            Mensaje = "No se pudo guardar el registro (posiblemente ya existe)"
                        });
                        resultado.RegistrosConErrores++;
                    }
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add(new ErrorImportacionDTO
                    {
                        Fila = fila,
                        Campo = "General",
                        Mensaje = $"Error al procesar: {ex.Message}"
                    });
                    resultado.RegistrosConErrores++;
                }
            }

            return resultado;
        }

        // Método para generar una plantilla de Excel con los encabezados de las columnas y datos de ejemplo opcionales
        public byte[] GenerarPlantilla(string nombreHoja, string[] columnas, string[][]? datosEjemplo = null)
        {
            using var package = new ExcelPackage(); // Crear un nuevo paquete de Excel
            var worksheet = package.Workbook.Worksheets.Add(nombreHoja); // Agregar una nueva hoja de trabajo con el nombre especificado

            // Crear encabezados
            for (int i = 0; i < columnas.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = columnas[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Agregar datos de ejemplo si se proporcionan
            if (datosEjemplo != null)
            {
                for (int fila = 0; fila < datosEjemplo.Length; fila++)
                {
                    for (int col = 0; col < datosEjemplo[fila].Length; col++)
                    {
                        worksheet.Cells[fila + 2, col + 1].Value = datosEjemplo[fila][col];
                    }
                }
            }

            // Ajustar el ancho de las columnas automáticamente para que se vean bien los encabezados y datos
            worksheet.Cells.AutoFitColumns();

            // Devolver el archivo Excel como un arreglo de bytes para que pueda ser descargado por el cliente
            return package.GetAsByteArray();
        }

        // Método privado para mapear una fila del Excel a un DTO genérico utilizando reflexión (basado en los nombres de las columnas)
        private TDto MapearFilaADto<TDto>(ExcelWorksheet worksheet, int fila, string[] columnas)
            where TDto : class, new()
        {
            // Crear una nueva instancia del DTO genérico y obtener las propiedades de la clase DTO
            var dto = new TDto();
            var propiedades = typeof(TDto).GetProperties();

            // Iterar sobre las columnas esperadas, buscar la propiedad correspondiente en el DTO
            // y asignar el valor de la celda convertida al tipo de la propiedad
            for (int i = 0; i < columnas.Length; i++)
            {
                var nombrePropiedad = columnas[i];
                var propiedad = propiedades.FirstOrDefault(p =>
                    p.Name.Equals(nombrePropiedad, StringComparison.OrdinalIgnoreCase));

                if (propiedad != null && propiedad.CanWrite)
                {
                    var valorCelda = worksheet.Cells[fila, i + 1].Text?.Trim();
                    var valorConvertido = ConvertirValor(valorCelda, propiedad.PropertyType);
                    propiedad.SetValue(dto, valorConvertido);
                }
            }

            return dto; // Devolver el DTO mapeado con los valores de la fila del Excel
        }

        // Método privado para convertir el valor de una celda del Excel al tipo de dato de la propiedad del DTO, manejando tipos comunes y valores nulos
        private object? ConvertirValor(string? valor, Type tipoDestino)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return tipoDestino.IsValueType ? Activator.CreateInstance(tipoDestino) : null;

            try
            {
                var tipoReal = Nullable.GetUnderlyingType(tipoDestino) ?? tipoDestino;

                if (tipoReal == typeof(int))
                    return int.Parse(valor);
                if (tipoReal == typeof(decimal))
                    return decimal.Parse(valor);
                if (tipoReal == typeof(double))
                    return double.Parse(valor);
                if (tipoReal == typeof(DateTime))
                    return DateTime.Parse(valor);
                if (tipoReal == typeof(bool))
                    return bool.Parse(valor);
                if (tipoReal.IsEnum)
                    return Enum.Parse(tipoReal, valor, true);

                return valor;
            }
            catch
            {
                return tipoDestino.IsValueType ? Activator.CreateInstance(tipoDestino) : null;
            }
        }
    }
}
