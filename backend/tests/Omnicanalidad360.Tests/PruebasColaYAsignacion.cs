using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Datos;
using Omnicanalidad360.Api.Dominio;
using Omnicanalidad360.Api.Simulaciones.Telefonia;

namespace Omnicanalidad360.Tests;

/// <summary>
/// Criterios CA-1.1, CA-1.2, CA-1.3, CA-2.1 y CA-2.2 del PRD.
/// </summary>
public class PruebasColaYAsignacion : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly ContextoOmnicanalidad _contexto;
    private readonly ServicioLlamadas _servicio;
    private readonly SimuladorTelefonia _telefonia;

    public PruebasColaYAsignacion()
    {
        // Base en memoria: cada prueba arranca limpia.
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<ContextoOmnicanalidad>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new ContextoOmnicanalidad(opciones);
        _contexto.Database.EnsureCreated();
        DatosSemilla.Cargar(_contexto);

        _servicio = new ServicioLlamadas(_contexto);
        _telefonia = new SimuladorTelefonia(_contexto, _servicio);
    }

    [Fact(DisplayName = "CA-1.1: el documento marcado queda guardado y la llamada pasa a la cola")]
    public async Task DocumentoMarcadoQuedaEnLaLlamada()
    {
        var llamada = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000001");

        Assert.Equal("1000000001", llamada.DocumentoIvr);
        Assert.Equal(EstadoLlamada.EnCola, llamada.Estado);
    }

    [Theory(DisplayName = "CA-1.2: si no marca o marca algo invalido, el documento queda vacio")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]            // menos de 5 digitos
    [InlineData("1234567890123")]  // mas de 12 digitos
    [InlineData("10000ABC01")]     // no son solo digitos
    public async Task DocumentoInvalidoQuedaVacio(string? marcado)
    {
        var llamada = await _telefonia.RecibirLlamadaAsync("+573001234567", marcado);

        Assert.Null(llamada.DocumentoIvr);
        Assert.Equal(EstadoLlamada.EnCola, llamada.Estado);
    }

    [Fact(DisplayName = "CA-1.3: toda llamada deja constancia del aviso de grabacion")]
    public async Task AvisoDeGrabacionSiempreQuedaRegistrado()
    {
        var llamada = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000002");

        Assert.True(llamada.AvisoGrabacionReproducido);
    }

    [Fact(DisplayName = "CA-2.1: suena primero en el agente que lleva mas tiempo disponible")]
    public async Task LaLlamadaVaAlAgenteQueLlevaMasTiempoLibre()
    {
        var ahora = DateTimeOffset.UtcNow.ToOffset(DatosSemilla.ZonaBogota);

        // AG-01 lleva 40 segundos disponible y AG-02 lleva 10.
        await PonerDisponible("AG-01", ahora.AddSeconds(-40));
        await PonerDisponible("AG-02", ahora.AddSeconds(-10));

        var llamada = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000003");

        Assert.Equal("AG-01", llamada.IdAgente);
    }

    [Fact(DisplayName = "CA-2.2: sin agentes disponibles la llamada espera en cola")]
    public async Task SinAgentesLaLlamadaEsperaEnCola()
    {
        var llamada = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000004");

        Assert.Null(llamada.IdAgente);
        Assert.Equal(EstadoLlamada.EnCola, llamada.Estado);
    }

    [Fact(DisplayName = "CA-2.2: si el paciente cuelga en cola, queda abandonada con su espera")]
    public async Task PacienteQueCuelgaEnColaQuedaAbandonada()
    {
        var entrante = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000005");

        var abandonada = await _servicio.AbandonarAsync(entrante.IdLlamada);

        Assert.NotNull(abandonada);
        Assert.Equal(EstadoLlamada.Abandonada, abandonada.Estado);
        Assert.True(abandonada.SegundosEspera >= 0);
        Assert.Null(abandonada.IdAgente);
    }

    [Fact(DisplayName = "Al contestar, la llamada pasa a en_curso y el agente a en_llamada")]
    public async Task ContestarDejaLaLlamadaEnCurso()
    {
        await PonerDisponible("AG-01", DateTimeOffset.UtcNow.AddSeconds(-30));
        var entrante = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000006");

        var contestada = await _servicio.ContestarAsync(entrante.IdLlamada, "AG-01");

        Assert.NotNull(contestada);
        Assert.Equal(EstadoLlamada.EnCurso, contestada.Estado);
        Assert.NotNull(contestada.HoraContesta);

        var agente = await _contexto.Usuarios.FindAsync("AG-01");
        Assert.Equal(EstadoAgente.EnLlamada, agente!.EstadoAgente);
    }

    [Fact(DisplayName = "Un agente no puede contestar una llamada que no le sonó")]
    public async Task AgenteNoContestaLlamadaAjena()
    {
        await PonerDisponible("AG-01", DateTimeOffset.UtcNow.AddSeconds(-30));
        var entrante = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000007");

        var resultado = await _servicio.ContestarAsync(entrante.IdLlamada, "AG-05");

        Assert.Null(resultado);
    }

    [Fact(DisplayName = "Al colgar, el agente vuelve a estar disponible")]
    public async Task ColgarDevuelveElAgenteADisponible()
    {
        await PonerDisponible("AG-01", DateTimeOffset.UtcNow.AddSeconds(-30));
        var entrante = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000008");
        await _servicio.ContestarAsync(entrante.IdLlamada, "AG-01");

        var finalizada = await _servicio.ColgarAsync(entrante.IdLlamada);

        Assert.NotNull(finalizada);
        Assert.Equal(EstadoLlamada.Finalizada, finalizada.Estado);
        Assert.NotNull(finalizada.HoraFin);

        var agente = await _contexto.Usuarios.FindAsync("AG-01");
        Assert.Equal(EstadoAgente.Disponible, agente!.EstadoAgente);
    }

    [Fact(DisplayName = "La llamada que esperaba entra apenas un agente se pone disponible")]
    public async Task LlamadaEnEsperaEntraCuandoHayAgente()
    {
        var entrante = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000009");
        Assert.Null(entrante.IdAgente);

        await _servicio.CambiarEstadoAgenteAsync("AG-03", EstadoAgente.Disponible);

        var llamada = await _contexto.Llamadas.FindAsync(entrante.IdLlamada);
        Assert.Equal("AG-03", llamada!.IdAgente);
    }

    [Fact(DisplayName = "Un agente en llamada no puede ponerse en pausa")]
    public async Task AgenteEnLlamadaNoCambiaDeEstado()
    {
        await PonerDisponible("AG-01", DateTimeOffset.UtcNow.AddSeconds(-30));
        var entrante = await _telefonia.RecibirLlamadaAsync("+573001234567", "1000000010");
        await _servicio.ContestarAsync(entrante.IdLlamada, "AG-01");

        await _servicio.CambiarEstadoAgenteAsync("AG-01", EstadoAgente.Pausa);

        var agente = await _contexto.Usuarios.FindAsync("AG-01");
        Assert.Equal(EstadoAgente.EnLlamada, agente!.EstadoAgente);
    }

    private async Task PonerDisponible(string idAgente, DateTimeOffset desde)
    {
        var agente = await _contexto.Usuarios.FindAsync(idAgente);
        agente!.EstadoAgente = EstadoAgente.Disponible;
        agente.EnEstadoDesde = desde;
        await _contexto.SaveChangesAsync();
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
        GC.SuppressFinalize(this);
    }
}
