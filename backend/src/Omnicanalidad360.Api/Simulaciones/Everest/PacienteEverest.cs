namespace Omnicanalidad360.Api.Simulaciones.Everest;

public static class EstadoDerechos
{
    public const string Vigentes = "vigentes";
    public const string NoVigentes = "no_vigentes";
    public const string SinValidar = "sin_validar";
}

/// <summary>
/// Copia local de lo que responderia Everest.
/// Esta tabla reemplaza al ambiente de pruebas de Everest mientras no haya acceso.
/// Cuando llegue el acceso real, se cambia el simulador y la consola no se toca.
/// Todos los pacientes son inventados (RN-12).
/// </summary>
public class PacienteEverest
{
    public int Id { get; set; }
    public string TipoDocumento { get; set; } = "CC";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? TelefonoAlterno { get; set; }
    public string? Sede { get; set; }
    public string Eps { get; set; } = string.Empty;

    /// <summary>vigentes, no_vigentes o sin_validar.</summary>
    public string Derechos { get; set; } = EstadoDerechos.Vigentes;
}
