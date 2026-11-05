namespace Omnicanalidad360.Api.Dominio;

/// <summary>
/// Estados de la llamada segun la seccion 8 del PRD.
/// </summary>
public static class EstadoLlamada
{
    public const string EnIvr = "en_ivr";
    public const string EnCola = "en_cola";
    public const string EnCurso = "en_curso";
    public const string Finalizada = "finalizada";
    public const string Abandonada = "abandonada";
}

/// <summary>
/// Una comunicacion telefonica que entra a la linea.
/// </summary>
public class Llamada
{
    public string IdLlamada { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Unico valor en v1: NEPS_IMAGENOLOGIA.</summary>
    public string Linea { get; set; } = Lineas.NepsImagenologia;

    /// <summary>Telefono del paciente en formato E.164.</summary>
    public string TelefonoOrigen { get; set; } = string.Empty;

    /// <summary>Documento marcado por teclado en el IVR. Vacio si el paciente no marco.</summary>
    public string? DocumentoIvr { get; set; }

    public DateTimeOffset HoraEntrada { get; set; }
    public DateTimeOffset? HoraContesta { get; set; }
    public DateTimeOffset? HoraFin { get; set; }

    /// <summary>Lo calcula la plataforma; nadie lo edita (RN-8).</summary>
    public int SegundosEspera { get; set; }

    public string Estado { get; set; } = EstadoLlamada.EnIvr;

    public string? IdAgente { get; set; }

    /// <summary>Evidencia de RN-6: toda llamada que llega a un agente paso por el aviso.</summary>
    public bool AvisoGrabacionReproducido { get; set; }

    /// <summary>Momento en que la llamada entro a la cola, para ordenar la espera.</summary>
    public DateTimeOffset? HoraEncolada { get; set; }
}

public static class Lineas
{
    public const string NepsImagenologia = "NEPS_IMAGENOLOGIA";
}
