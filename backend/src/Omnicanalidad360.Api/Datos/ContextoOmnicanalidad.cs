using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Dominio;
using Omnicanalidad360.Api.Simulaciones.Everest;
using Omnicanalidad360.Api.Simulaciones.Lumier;

namespace Omnicanalidad360.Api.Datos;

/// <summary>
/// Base de datos local en SQLite. Pensada para migrar despues a SQL Server
/// sin cambiar las entidades ni la consola.
/// </summary>
public class ContextoOmnicanalidad(DbContextOptions<ContextoOmnicanalidad> opciones)
    : DbContext(opciones)
{
    public DbSet<Llamada> Llamadas => Set<Llamada>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Tipificacion> Tipificaciones => Set<Tipificacion>();
    public DbSet<PacienteEverest> PacientesEverest => Set<PacienteEverest>();
    public DbSet<CupoLumier> CuposLumier => Set<CupoLumier>();

    /// <summary>
    /// Todas las fechas se guardan de forma ordenable y vuelven con la zona
    /// horaria de Bogota (ver ConversorFechaBogota).
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configuracion)
    {
        configuracion.Properties<DateTimeOffset>().HaveConversion<ConversorFechaBogota>();
        configuracion.Properties<DateTimeOffset?>().HaveConversion<ConversorFechaBogotaOpcional>();
    }

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<Llamada>().HasKey(l => l.IdLlamada);
        modelo.Entity<Usuario>().HasKey(u => u.IdUsuario);

        // Un paciente de prueba por documento, igual que en Everest.
        modelo.Entity<PacienteEverest>()
            .HasIndex(p => p.NumeroDocumento)
            .IsUnique();

        modelo.Entity<CupoLumier>().Ignore(c => c.Disponible);
    }
}
