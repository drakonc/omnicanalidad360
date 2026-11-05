using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Datos;

namespace Omnicanalidad360.Api.Dominio;

/// <summary>
/// Cola de llamadas y asignacion a los agentes (capacidad C2 del PRD).
/// Esta logica es de la plataforma, no de la telefonia: no cambia cuando
/// el simulador de llamadas se reemplace por la telefonia real.
/// </summary>
public class ServicioLlamadas(ContextoOmnicanalidad contexto)
{
    private static DateTimeOffset Ahora =>
        DateTimeOffset.UtcNow.ToOffset(DatosSemilla.ZonaBogota);

    /// <summary>
    /// Pone la llamada en cola una vez termino el IVR y la entrega
    /// al agente disponible que lleva mas tiempo libre (CA-2.1).
    /// </summary>
    public async Task EncolarAsync(Llamada llamada)
    {
        llamada.Estado = EstadoLlamada.EnCola;
        llamada.HoraEncolada = Ahora;
        await contexto.SaveChangesAsync();
        await AsignarPendientesAsync();
    }

    /// <summary>
    /// Entrega las llamadas que esperan en cola a los agentes disponibles,
    /// la mas antigua primero y al agente que lleva mas tiempo libre.
    /// Se llama cada vez que cambia algo: entra una llamada, un agente se
    /// pone disponible o un agente cuelga.
    /// </summary>
    public async Task AsignarPendientesAsync()
    {
        var enCola = await contexto.Llamadas
            .Where(l => l.Estado == EstadoLlamada.EnCola && l.IdAgente == null)
            .OrderBy(l => l.HoraEncolada)
            .ToListAsync();

        if (enCola.Count == 0)
        {
            return;
        }

        var disponibles = await contexto.Usuarios
            .Where(u => u.Rol == RolUsuario.Agente && u.EstadoAgente == EstadoAgente.Disponible)
            .OrderBy(u => u.EnEstadoDesde)
            .ToListAsync();

        var cuantas = Math.Min(enCola.Count, disponibles.Count);

        for (var i = 0; i < cuantas; i++)
        {
            // La llamada queda sonando en la consola de ese agente.
            // Sigue en cola hasta que el agente conteste.
            enCola[i].IdAgente = disponibles[i].IdUsuario;
        }

        await contexto.SaveChangesAsync();
    }

    /// <summary>
    /// El agente contesta: la llamada pasa a en_curso y el agente a en_llamada.
    /// </summary>
    public async Task<Llamada?> ContestarAsync(string idLlamada, string idAgente)
    {
        var llamada = await contexto.Llamadas.FindAsync(idLlamada);

        if (llamada is null
            || llamada.Estado != EstadoLlamada.EnCola
            || llamada.IdAgente != idAgente)
        {
            return null;
        }

        var ahora = Ahora;
        llamada.Estado = EstadoLlamada.EnCurso;
        llamada.HoraContesta = ahora;
        llamada.SegundosEspera = SegundosEntre(llamada.HoraEntrada, ahora);

        var agente = await contexto.Usuarios.FindAsync(idAgente);
        if (agente is not null)
        {
            agente.EstadoAgente = EstadoAgente.EnLlamada;
            agente.EnEstadoDesde = ahora;
        }

        await contexto.SaveChangesAsync();
        return llamada;
    }

    /// <summary>
    /// Alguien cuelga una llamada en curso: queda finalizada y el agente
    /// vuelve a estar disponible.
    /// </summary>
    public async Task<Llamada?> ColgarAsync(string idLlamada)
    {
        var llamada = await contexto.Llamadas.FindAsync(idLlamada);

        if (llamada is null || llamada.Estado != EstadoLlamada.EnCurso)
        {
            return null;
        }

        var ahora = Ahora;
        llamada.Estado = EstadoLlamada.Finalizada;
        llamada.HoraFin = ahora;

        if (llamada.IdAgente is not null)
        {
            var agente = await contexto.Usuarios.FindAsync(llamada.IdAgente);
            if (agente is not null)
            {
                agente.EstadoAgente = EstadoAgente.Disponible;
                agente.EnEstadoDesde = ahora;
            }
        }

        await contexto.SaveChangesAsync();
        await AsignarPendientesAsync();
        return llamada;
    }

    /// <summary>
    /// El paciente cuelga antes de que un agente conteste (CA-2.2).
    /// </summary>
    public async Task<Llamada?> AbandonarAsync(string idLlamada)
    {
        var llamada = await contexto.Llamadas.FindAsync(idLlamada);

        if (llamada is null
            || (llamada.Estado != EstadoLlamada.EnCola && llamada.Estado != EstadoLlamada.EnIvr))
        {
            return null;
        }

        var ahora = Ahora;
        llamada.Estado = EstadoLlamada.Abandonada;
        llamada.HoraFin = ahora;
        llamada.SegundosEspera = SegundosEntre(llamada.HoraEntrada, ahora);
        llamada.IdAgente = null;

        await contexto.SaveChangesAsync();
        return llamada;
    }

    /// <summary>
    /// Cambia el estado del agente. Al ponerse disponible, se le entrega
    /// de inmediato una llamada que este esperando en cola.
    /// </summary>
    public async Task<Usuario?> CambiarEstadoAgenteAsync(string idAgente, string nuevoEstado)
    {
        var agente = await contexto.Usuarios.FindAsync(idAgente);

        if (agente is null || agente.Rol != RolUsuario.Agente)
        {
            return null;
        }

        // Un agente en llamada no puede cambiar de estado hasta colgar.
        if (agente.EstadoAgente == EstadoAgente.EnLlamada)
        {
            return agente;
        }

        agente.EstadoAgente = nuevoEstado;
        agente.EnEstadoDesde = Ahora;
        await contexto.SaveChangesAsync();

        if (nuevoEstado == EstadoAgente.Disponible)
        {
            await AsignarPendientesAsync();
        }

        return agente;
    }

    /// <summary>
    /// Los segundos los calcula la plataforma; nadie los edita (RN-8).
    /// </summary>
    private static int SegundosEntre(DateTimeOffset inicio, DateTimeOffset fin) =>
        Math.Max(0, (int)(fin - inicio).TotalSeconds);
}
