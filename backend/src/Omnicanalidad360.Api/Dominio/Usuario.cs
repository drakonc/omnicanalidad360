namespace Omnicanalidad360.Api.Dominio;

public static class RolUsuario
{
    public const string Agente = "agente";
    public const string Supervisor = "supervisor";
    public const string Administrador = "administrador";
}

public static class EstadoAgente
{
    public const string Disponible = "disponible";
    public const string EnLlamada = "en_llamada";
    public const string Pausa = "pausa";
    public const string Desconectado = "desconectado";
}

/// <summary>
/// Agentes, supervisor y administradores (seccion 8 del PRD).
/// </summary>
public class Usuario
{
    public string IdUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = RolUsuario.Agente;

    /// <summary>Solo aplica a los agentes.</summary>
    public string? EstadoAgente { get; set; }

    /// <summary>
    /// Desde cuando el agente esta en su estado actual.
    /// Sirve para entregar la llamada al disponible que lleva mas tiempo libre (CA-2.1).
    /// </summary>
    public DateTimeOffset? EnEstadoDesde { get; set; }
}
