namespace Omnicanalidad360.Api.Simulaciones.Lumier;

/// <summary>
/// Tabla de cupos que reemplaza al ambiente de pruebas de LUMIER,
/// con los mismos campos de la Cita del PRD (seccion 8).
/// Cuando llegue el acceso real, se cambia el simulador y la consola no se toca.
/// </summary>
public class CupoLumier
{
    public int Id { get; set; }
    public string Examen { get; set; } = string.Empty;
    public string? CodigoCups { get; set; }
    public string Sede { get; set; } = string.Empty;
    public DateTimeOffset FechaHora { get; set; }

    /// <summary>Codigo que devuelve LUMIER cuando el cupo ya fue tomado.</summary>
    public string? IdCita { get; set; }

    public bool Disponible => IdCita is null;
}
