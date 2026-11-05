using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Datos;
using Omnicanalidad360.Api.Dominio;

namespace Omnicanalidad360.Api.Endpoints;

/// <summary>
/// Lo que necesitan la vista de supervisor y la pantalla de tipificacion.
/// Devuelve lo que la plataforma ya sabe de verdad; lo que todavia no
/// existe (nivel de servicio, grabaciones) lo marca la pantalla como
/// dato de prueba.
/// </summary>
public static class EndpointsSupervision
{
    public static void MapearEndpointsSupervision(this WebApplication app)
    {
        var tablero = app.MapGroup("/api/supervisor").WithTags("Supervision");

        // Estado de la operacion del dia.
        tablero.MapGet("/tablero", async (ContextoOmnicanalidad contexto) =>
        {
            var desdeMedianoche = DateTimeOffset.UtcNow
                .ToOffset(DatosSemilla.ZonaBogota).Date;

            var agentes = await contexto.Usuarios
                .Where(u => u.Rol == RolUsuario.Agente)
                .OrderBy(u => u.IdUsuario)
                .Select(u => new AgenteEnTablero(
                    u.IdUsuario,
                    u.Nombre,
                    u.EstadoAgente ?? EstadoAgente.Desconectado,
                    u.EnEstadoDesde))
                .ToListAsync();

            var llamadasDeHoy = contexto.Llamadas.Where(l => l.HoraEntrada >= desdeMedianoche);

            var atendidas = await llamadasDeHoy
                .CountAsync(l => l.Estado == EstadoLlamada.Finalizada);
            var abandonadas = await llamadasDeHoy
                .CountAsync(l => l.Estado == EstadoLlamada.Abandonada);
            var enCurso = await llamadasDeHoy
                .CountAsync(l => l.Estado == EstadoLlamada.EnCurso);
            var enCola = await contexto.Llamadas
                .CountAsync(l => l.Estado == EstadoLlamada.EnCola);

            // La espera mas larga de las llamadas que siguen en cola ahora mismo.
            var ahora = DateTimeOffset.UtcNow.ToOffset(DatosSemilla.ZonaBogota);
            var entradasEnCola = await contexto.Llamadas
                .Where(l => l.Estado == EstadoLlamada.EnCola)
                .Select(l => l.HoraEntrada)
                .ToListAsync();

            var esperaMasLarga = entradasEnCola.Count == 0
                ? 0
                : entradasEnCola.Max(entrada => (int)(ahora - entrada).TotalSeconds);

            // Promedio de conversacion de las llamadas terminadas hoy.
            var duraciones = await llamadasDeHoy
                .Where(l => l.Estado == EstadoLlamada.Finalizada
                    && l.HoraContesta != null
                    && l.HoraFin != null)
                .Select(l => new { l.HoraContesta, l.HoraFin })
                .ToListAsync();

            var tmoPromedio = duraciones.Count == 0
                ? 0
                : (int)duraciones.Average(d => (d.HoraFin!.Value - d.HoraContesta!.Value).TotalSeconds);

            return Results.Ok(new RespuestaTablero(
                agentes,
                enCola,
                enCurso,
                atendidas,
                abandonadas,
                esperaMasLarga,
                tmoPromedio));
        });

        // Catalogo de tipificacion, para la pantalla de cierre de la gestion.
        app.MapGet("/api/catalogo/tipificaciones", async (ContextoOmnicanalidad contexto) =>
        {
            var catalogo = await contexto.Tipificaciones
                .Where(t => t.Activa)
                .OrderBy(t => t.Nivel1).ThenBy(t => t.Nivel2)
                .Select(t => new { t.Nivel1, t.Nivel2, t.ExigeCita })
                .ToListAsync();

            return Results.Ok(catalogo);
        }).WithTags("Supervision");
    }
}

public record AgenteEnTablero(
    string IdUsuario,
    string Nombre,
    string EstadoAgente,
    DateTimeOffset? EnEstadoDesde);

public record RespuestaTablero(
    List<AgenteEnTablero> Agentes,
    int LlamadasEnCola,
    int LlamadasEnCurso,
    int AtendidasHoy,
    int AbandonadasHoy,
    int EsperaMasLargaSegundos,
    int TmoPromedioSegundos);
