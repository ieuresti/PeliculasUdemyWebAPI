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

        // Método genérico para importar datos desde Excel
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
                    // Mapear con validación de conversión
                    var (dto, erroresConversion) = MapearFilaADtoConValidacion<TDto>(worksheet, fila, columnasEsperadas);

                    // Si hubo errores de conversión de tipos, agregarlos y continuar con la siguiente fila
                    if (erroresConversion.Any())
                    {
                        resultado.Errores.AddRange(erroresConversion.Select(e => new ErrorImportacionDTO
                        {
                            Fila = fila,
                            Campo = e.Campo,
                            Mensaje = e.Mensaje
                        }));
                        resultado.RegistrosConErrores++;
                        continue;
                    }

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

        // Método para mapear una fila del Excel a un DTO, intentando convertir los valores y capturando errores específicos de conversión para cada campo
        private (TDto dto, List<(string Campo, string Mensaje)> errores) MapearFilaADtoConValidacion<TDto>(
            ExcelWorksheet worksheet,
            int fila,
            string[] columnas)
            where TDto : class, new()
        {
            var dto = new TDto();
            var propiedades = typeof(TDto).GetProperties();
            var errores = new List<(string Campo, string Mensaje)>();

            // Iterar sobre cada columna esperada
            for (int i = 0; i < columnas.Length; i++)
            {
                var nombrePropiedad = columnas[i];
                var propiedad = propiedades.FirstOrDefault(p =>
                    p.Name.Equals(nombrePropiedad, StringComparison.OrdinalIgnoreCase)); // Buscar la propiedad del DTO que coincide con el nombre de la columna (ignorando mayúsculas/minúsculas)

                if (propiedad != null && propiedad.CanWrite)
                {
                    var valorCelda = worksheet.Cells[fila, i + 1].Text?.Trim();

                    // Intentar convertir el valor de la celda al tipo de dato de la propiedad, capturando errores específicos de conversión
                    var (exito, valorConvertido, mensajeError) = TryConvertirValor(valorCelda, propiedad.PropertyType, nombrePropiedad);

                    if (!exito)
                    {
                        errores.Add((nombrePropiedad, mensajeError!)); // Si la conversión falla, agregar un error específico para ese campo
                    }
                    else
                    {
                        propiedad.SetValue(dto, valorConvertido); // Si la conversión es exitosa, asignar el valor convertido a la propiedad del DTO
                    }
                }
            }

            return (dto, errores); // Devolver el DTO mapeado junto con la lista de errores de conversión (si los hubo) para que puedan ser reportados al usuario
        }

        // Método para intentar convertir un valor de celda a un tipo de dato específico, manejando tipos comunes como int, decimal, DateTime, bool y enums, y devolviendo mensajes de error claros en caso de fallos de conversión
        private (bool exito, object? valor, string? mensajeError) TryConvertirValor(
            string? valor,
            Type tipoDestino,
            string nombreCampo)
        {
            // Permitir valores vacíos para tipos nullable
            if (string.IsNullOrWhiteSpace(valor))
            {
                var esNullable = Nullable.GetUnderlyingType(tipoDestino) != null || !tipoDestino.IsValueType;
                if (esNullable)
                {
                    return (true, null, null);
                }
                else
                {
                    return (false, null, $"El campo '{nombreCampo}' no puede estar vacío");
                }
            }

            try
            {
                // Obtener el tipo real en caso de que sea nullable para manejar la conversión correctamente
                var tipoReal = Nullable.GetUnderlyingType(tipoDestino) ?? tipoDestino;

                // Manejar tipos comunes con mensajes de error específicos para cada tipo en caso de fallos de conversión
                if (tipoReal == typeof(int))
                {
                    if (int.TryParse(valor, out var resultado))
                        return (true, resultado, null);
                    return (false, null, $"'{valor}' no es un número entero válido para '{nombreCampo}'");
                }

                if (tipoReal == typeof(decimal))
                {
                    if (decimal.TryParse(valor, out var resultado))
                        return (true, resultado, null);
                    return (false, null, $"'{valor}' no es un número decimal válido para '{nombreCampo}'");
                }

                if (tipoReal == typeof(double))
                {
                    if (double.TryParse(valor, out var resultado))
                        return (true, resultado, null);
                    return (false, null, $"'{valor}' no es un número válido para '{nombreCampo}'");
                }

                if (tipoReal == typeof(DateTime))
                {
                    if (DateTime.TryParse(valor, out var resultado))
                        return (true, resultado, null);
                    return (false, null, $"'{valor}' no es una fecha válida para '{nombreCampo}'. Use formato: dd/MM/yyyy o yyyy-MM-dd");
                }

                if (tipoReal == typeof(bool))
                {
                    if (bool.TryParse(valor, out var resultado))
                        return (true, resultado, null);
                    return (false, null, $"'{valor}' no es un valor booleano válido para '{nombreCampo}'. Use: true/false o sí/no");
                }

                if (tipoReal.IsEnum)
                {
                    if (Enum.TryParse(tipoReal, valor, true, out var resultado))
                        return (true, resultado, null);

                    var valoresPermitidos = string.Join(", ", Enum.GetNames(tipoReal));
                    return (false, null, $"'{valor}' no es válido para '{nombreCampo}'. Valores permitidos: {valoresPermitidos}");
                }

                // Para otros tipos, devolver el valor como string y dejar que la validación posterior maneje cualquier error de formato o tipo
                return (true, valor, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error al convertir '{valor}' para '{nombreCampo}': {ex.Message}");
            }
        }
    }
}
