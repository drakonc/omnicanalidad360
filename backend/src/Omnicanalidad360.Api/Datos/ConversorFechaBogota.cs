using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Omnicanalidad360.Api.Datos;

/// <summary>
/// SQLite no sabe comparar ni ordenar fechas con zona horaria, y la cola
/// necesita ordenar por hora. Por eso la fecha se guarda como un numero
/// (el instante exacto) y al leerla vuelve con la zona horaria de Bogota,
/// que es la que usa todo el PRD.
///
/// Cuando se migre a SQL Server este conversor se puede quitar.
/// </summary>
public class ConversorFechaBogota() : ValueConverter<DateTimeOffset, long>(
    fecha => fecha.UtcTicks,
    ticks => new DateTimeOffset(ticks, TimeSpan.Zero).ToOffset(DatosSemilla.ZonaBogota));

public class ConversorFechaBogotaOpcional() : ValueConverter<DateTimeOffset?, long?>(
    fecha => fecha.HasValue ? fecha.Value.UtcTicks : null,
    ticks => ticks.HasValue
        ? new DateTimeOffset(ticks.Value, TimeSpan.Zero).ToOffset(DatosSemilla.ZonaBogota)
        : null);
