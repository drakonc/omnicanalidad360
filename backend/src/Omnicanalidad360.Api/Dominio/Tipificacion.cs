namespace Omnicanalidad360.Api.Dominio;

/// <summary>
/// Catalogo de causales de la llamada, en dos niveles (seccion 8 del PRD).
/// El Nivel 1 viene de la tipificacion de septiembre de 2026.
/// El Nivel 2 es provisional: esta por validar con el supervisor.
/// </summary>
public class Tipificacion
{
    public int Id { get; set; }
    public string Nivel1 { get; set; } = string.Empty;
    public string? Nivel2 { get; set; }

    /// <summary>Si es true, la gestion no se puede cerrar sin cita (RN-5).</summary>
    public bool ExigeCita { get; set; }

    public bool Activa { get; set; } = true;
}
