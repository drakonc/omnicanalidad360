using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Datos;
using Omnicanalidad360.Api.Dominio;
using Omnicanalidad360.Api.Simulaciones.Everest;

namespace Omnicanalidad360.Tests;

/// <summary>
/// Datos de prueba de la seccion 12 del PRD.
/// </summary>
public class PruebasDatosSemilla : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly ContextoOmnicanalidad _contexto;

    public PruebasDatosSemilla()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<ContextoOmnicanalidad>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new ContextoOmnicanalidad(opciones);
        _contexto.Database.EnsureCreated();
        DatosSemilla.Cargar(_contexto);
    }

    [Fact(DisplayName = "Hay 5 agentes, 1 supervisor y 2 administradores")]
    public void LosOchoUsuariosEstanCargados()
    {
        Assert.Equal(5, _contexto.Usuarios.Count(u => u.Rol == RolUsuario.Agente));
        Assert.Equal(1, _contexto.Usuarios.Count(u => u.Rol == RolUsuario.Supervisor));
        Assert.Equal(2, _contexto.Usuarios.Count(u => u.Rol == RolUsuario.Administrador));
    }

    [Fact(DisplayName = "Hay 30 pacientes: 26 con derechos vigentes y 4 sin derechos")]
    public void LosPacientesDePruebaEstanCargados()
    {
        Assert.Equal(30, _contexto.PacientesEverest.Count());
        Assert.Equal(4, _contexto.PacientesEverest.Count(p => p.Derechos == EstadoDerechos.NoVigentes));
    }

    [Fact(DisplayName = "Hay 2 pacientes con un dato de contacto vacio")]
    public void HayPacientesConDatosIncompletos()
    {
        var incompletos = _contexto.PacientesEverest
            .Count(p => p.Correo == null || p.TelefonoAlterno == null);

        Assert.Equal(2, incompletos);
    }

    [Fact(DisplayName = "Los documentos 1000000098 y 1000000099 no existen a proposito")]
    public void LosDocumentosInexistentesNoEstan()
    {
        Assert.False(_contexto.PacientesEverest.Any(p => p.NumeroDocumento == "1000000098"));
        Assert.False(_contexto.PacientesEverest.Any(p => p.NumeroDocumento == "1000000099"));
    }

    [Fact(DisplayName = "Hay cupos para 3 examenes y 2 sedes en las 2 semanas siguientes")]
    public void LosCuposDePruebaEstanCargados()
    {
        Assert.Equal(3, _contexto.CuposLumier.Select(c => c.Examen).Distinct().Count());
        Assert.Equal(2, _contexto.CuposLumier.Select(c => c.Sede).Distinct().Count());
        Assert.True(_contexto.CuposLumier.Any());
    }

    [Fact(DisplayName = "El catalogo tiene los 8 niveles 1 de la tipificacion de septiembre")]
    public void ElCatalogoDeTipificacionEstaCargado()
    {
        string[] esperados =
        [
            "ASIGNACION", "INFORMACION", "ESCALAMIENTO - NO CITA", "CANCELACION",
            "CONFIRMACION", "PQR", "NO CONTACTO", "OTROS"
        ];

        var cargados = _contexto.Tipificaciones.Select(t => t.Nivel1).Distinct().ToList();

        Assert.Equal(esperados.Length, cargados.Count);
        Assert.All(esperados, e => Assert.Contains(e, cargados));
    }

    [Fact(DisplayName = "Cargar dos veces no duplica los datos de prueba")]
    public void CargarDosVecesNoDuplica()
    {
        DatosSemilla.Cargar(_contexto);

        Assert.Equal(30, _contexto.PacientesEverest.Count());
        Assert.Equal(8, _contexto.Usuarios.Count());
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
        GC.SuppressFinalize(this);
    }
}
