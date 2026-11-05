using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Datos;
using Omnicanalidad360.Api.Dominio;
using Omnicanalidad360.Api.Simulaciones.Telefonia;

namespace Omnicanalidad360.Api.Endpoints;

/// <summary>
/// Lo que la pantalla necesita para el hito 1: el panel que simula la llamada
/// y la consola del agente.
/// </summary>
public static class EndpointsConsola
{
    public static void MapearEndpointsConsola(this WebApplication app)
    {
        MapearSimulador(app);
        MapearConsola(app);
    }

    private static void MapearSimulador(WebApplication app)
    {
        var grupo = app.MapGroup("/api/simulacion").WithTags("Simulacion de la llamada");

        // Entra una llamada al numero de prueba.
        grupo.MapPost("/llamadas", async (
            SolicitudLlamadaEntrante solicitud,
            ITelefonia telefonia) =>
        {
            if (string.IsNullOrWhiteSpace(solicitud.TelefonoOrigen))
            {
                return Results.BadRequest(new { mensaje = "Falta el telefono de origen." });
            }

            var llamada = await telefonia.RecibirLlamadaAsync(
                solicitud.TelefonoOrigen,
                solicitud.DocumentoMarcado);

            return Results.Ok(new
            {
                llamada.IdLlamada,
                llamada.Estado,
                llamada.DocumentoIvr,
                avisoReproducido = SimuladorTelefonia.AvisoGrabacion
            });
        });

        // El paciente cuelga antes de que le contesten (CA-2.2).
        grupo.MapPost("/llamadas/{idLlamada}/colgar-paciente", async (
            string idLlamada,
            ServicioLlamadas servicio) =>
        {
            var llamada = await servicio.AbandonarAsync(idLlamada);
            return llamada is null
                ? Results.BadRequest(new { mensaje = "La llamada ya fue contestada o no existe." })
                : Results.Ok(new { llamada.IdLlamada, llamada.Estado, llamada.SegundosEspera });
        });

        // Documentos de prueba, para no tener que recordarlos de memoria.
        grupo.MapGet("/documentos-de-prueba", async (ContextoOmnicanalidad contexto) =>
        {
            var pacientes = await contexto.PacientesEverest
                .OrderBy(p => p.NumeroDocumento)
                .Select(p => new
                {
                    p.NumeroDocumento,
                    p.NombreCompleto,
                    p.Derechos
                })
                .ToListAsync();

            return Results.Ok(pacientes);
        });
    }

    private static void MapearConsola(WebApplication app)
    {
        var grupo = app.MapGroup("/api/consola").WithTags("Consola del agente");

        // Los 5 agentes, para elegir con cual se trabaja en la demo.
        grupo.MapGet("/agentes", async (ContextoOmnicanalidad contexto) =>
        {
            var agentes = await contexto.Usuarios
                .Where(u => u.Rol == RolUsuario.Agente)
                .OrderBy(u => u.IdUsuario)
                .Select(u => new { u.IdUsuario, u.Nombre, u.EstadoAgente })
                .ToListAsync();

            return Results.Ok(agentes);
        });

        // Todo lo que la consola del agente muestra en pantalla.
        grupo.MapGet("/{idAgente}", async (
            string idAgente,
            ContextoOmnicanalidad contexto,
            ServicioLlamadas servicio) =>
        {
            var agente = await contexto.Usuarios.FindAsync(idAgente);

            if (agente is null || agente.Rol != RolUsuario.Agente)
            {
                return Results.NotFound(new { mensaje = "Ese agente no existe." });
            }

            // Por si una llamada quedo esperando y este agente ya esta libre.
            await servicio.AsignarPendientesAsync();

            var llamada = await contexto.Llamadas
                .Where(l => l.IdAgente == idAgente
                    && (l.Estado == EstadoLlamada.EnCola || l.Estado == EstadoLlamada.EnCurso))
                .OrderBy(l => l.HoraEncolada)
                .FirstOrDefaultAsync();

            var enCola = await contexto.Llamadas
                .CountAsync(l => l.Estado == EstadoLlamada.EnCola && l.IdAgente == null);

            // Indicadores del turno: solo las llamadas de hoy.
            var desdeMedianoche = DateTimeOffset.UtcNow
                .ToOffset(DatosSemilla.ZonaBogota).Date;

            var atendidasHoy = await contexto.Llamadas.CountAsync(l =>
                l.IdAgente == idAgente
                && l.Estado == EstadoLlamada.Finalizada
                && l.HoraEntrada >= desdeMedianoche);

            var abandonadasHoy = await contexto.Llamadas.CountAsync(l =>
                l.Estado == EstadoLlamada.Abandonada
                && l.HoraEntrada >= desdeMedianoche);

            return Results.Ok(new RespuestaConsola(
                agente.IdUsuario,
                agente.Nombre,
                agente.EstadoAgente ?? EstadoAgente.Desconectado,
                agente.EnEstadoDesde,
                llamada is null ? null : new LlamadaEnConsola(
                    llamada.IdLlamada,
                    llamada.TelefonoOrigen,
                    llamada.DocumentoIvr,
                    llamada.Estado,
                    llamada.HoraEntrada,
                    llamada.HoraContesta,
                    llamada.AvisoGrabacionReproducido),
                enCola,
                atendidasHoy,
                abandonadasHoy));
        });

        // Cambiar entre disponible y pausa.
        grupo.MapPost("/{idAgente}/estado", async (
            string idAgente,
            SolicitudEstadoAgente solicitud,
            ServicioLlamadas servicio) =>
        {
            string[] permitidos =
            [
                EstadoAgente.Disponible,
                EstadoAgente.Pausa,
                EstadoAgente.Desconectado
            ];

            if (!permitidos.Contains(solicitud.Estado))
            {
                return Results.BadRequest(new { mensaje = "Ese estado no existe." });
            }

            var agente = await servicio.CambiarEstadoAgenteAsync(idAgente, solicitud.Estado);

            return agente is null
                ? Results.NotFound(new { mensaje = "Ese agente no existe." })
                : Results.Ok(new { agente.IdUsuario, agente.EstadoAgente });
        });

        // El agente contesta la llamada que le esta sonando.
        grupo.MapPost("/{idAgente}/contestar/{idLlamada}", async (
            string idAgente,
            string idLlamada,
            ServicioLlamadas servicio) =>
        {
            var llamada = await servicio.ContestarAsync(idLlamada, idAgente);
            return llamada is null
                ? Results.BadRequest(new { mensaje = "Esa llamada ya no esta disponible." })
                : Results.Ok(new { llamada.IdLlamada, llamada.Estado, llamada.SegundosEspera });
        });

        // El agente cuelga.
        grupo.MapPost("/{idAgente}/colgar/{idLlamada}", async (
            string idAgente,
            string idLlamada,
            ServicioLlamadas servicio) =>
        {
            var llamada = await servicio.ColgarAsync(idLlamada);
            return llamada is null
                ? Results.BadRequest(new { mensaje = "Esa llamada no esta en curso." })
                : Results.Ok(new { llamada.IdLlamada, llamada.Estado });
        });
    }
}

public record SolicitudLlamadaEntrante(string TelefonoOrigen, string? DocumentoMarcado);

public record SolicitudEstadoAgente(string Estado);

public record LlamadaEnConsola(
    string IdLlamada,
    string TelefonoOrigen,
    string? DocumentoIvr,
    string Estado,
    DateTimeOffset HoraEntrada,
    DateTimeOffset? HoraContesta,
    bool AvisoGrabacionReproducido);

public record RespuestaConsola(
    string IdAgente,
    string Nombre,
    string EstadoAgente,
    DateTimeOffset? EnEstadoDesde,
    LlamadaEnConsola? Llamada,
    int LlamadasEnCola,
    int AtendidasHoy,
    int AbandonadasHoy);
