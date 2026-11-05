using Omnicanalidad360.Api.Datos;
using Omnicanalidad360.Api.Dominio;

namespace Omnicanalidad360.Api.Simulaciones.Telefonia;

/// <summary>
/// Lo que haria la telefonia real: recibir la llamada, reproducir el aviso
/// de grabacion y capturar el documento por teclado (capacidad C1 del PRD).
///
/// Hoy lo simula un panel en la pantalla. Cuando llegue la telefonia de Azure,
/// se escribe otra implementacion de esta misma interfaz y la consola no se toca.
/// </summary>
public interface ITelefonia
{
    Task<Llamada> RecibirLlamadaAsync(string telefonoOrigen, string? documentoMarcado);
}

public class SimuladorTelefonia(ContextoOmnicanalidad contexto, ServicioLlamadas servicioLlamadas)
    : ITelefonia
{
    /// <summary>Lo que el IVR le dice al paciente antes de pedir el documento (RN-6).</summary>
    public const string AvisoGrabacion =
        "Esta llamada puede ser grabada con fines de calidad y seguridad.";

    /// <summary>El IVR da 2 intentos de 10 segundos para marcar el documento (CA-1.2).</summary>
    public const int IntentosDocumento = 2;

    /// <summary>
    /// Recibe la llamada, reproduce el aviso, valida el documento marcado
    /// y la deja en cola esperando agente.
    /// </summary>
    public async Task<Llamada> RecibirLlamadaAsync(string telefonoOrigen, string? documentoMarcado)
    {
        var llamada = new Llamada
        {
            TelefonoOrigen = telefonoOrigen,
            HoraEntrada = DateTimeOffset.UtcNow.ToOffset(DatosSemilla.ZonaBogota),
            Estado = EstadoLlamada.EnIvr,

            // El aviso se reproduce siempre, antes de pedir el documento.
            AvisoGrabacionReproducido = true,

            // Si el paciente no marca o marca algo invalido, el documento
            // queda vacio y la llamada igual pasa a cola (CA-1.2).
            DocumentoIvr = EsDocumentoValido(documentoMarcado) ? documentoMarcado : null
        };

        contexto.Llamadas.Add(llamada);
        await contexto.SaveChangesAsync();

        await servicioLlamadas.EncolarAsync(llamada);
        return llamada;
    }

    /// <summary>De 5 a 12 digitos, como define la seccion 8 del PRD.</summary>
    public static bool EsDocumentoValido(string? documento) =>
        !string.IsNullOrWhiteSpace(documento)
        && documento.Length is >= 5 and <= 12
        && documento.All(char.IsAsciiDigit);
}
