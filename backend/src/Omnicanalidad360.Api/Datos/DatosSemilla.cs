using Omnicanalidad360.Api.Dominio;
using Omnicanalidad360.Api.Simulaciones.Everest;
using Omnicanalidad360.Api.Simulaciones.Lumier;

namespace Omnicanalidad360.Api.Datos;

/// <summary>
/// Datos de prueba de la seccion 12 del PRD, cargados antes de la primera prueba
/// para que la consola se pueda recorrer desde el primer minuto.
/// Todos los pacientes son inventados (RN-12).
/// </summary>
public static class DatosSemilla
{
    /// <summary>Zona horaria de Bogota, usada en todo el PRD.</summary>
    public static readonly TimeSpan ZonaBogota = TimeSpan.FromHours(-5);

    public static void Cargar(ContextoOmnicanalidad contexto)
    {
        if (!contexto.Usuarios.Any())
        {
            contexto.Usuarios.AddRange(ConstruirUsuarios());
        }

        if (!contexto.Tipificaciones.Any())
        {
            contexto.Tipificaciones.AddRange(ConstruirTipificaciones());
        }

        if (!contexto.PacientesEverest.Any())
        {
            contexto.PacientesEverest.AddRange(ConstruirPacientes());
        }

        if (!contexto.CuposLumier.Any())
        {
            contexto.CuposLumier.AddRange(ConstruirCupos());
        }

        contexto.SaveChanges();
    }

    /// <summary>5 agentes, 1 supervisor y 2 administradores.</summary>
    private static IEnumerable<Usuario> ConstruirUsuarios()
    {
        var ahora = DateTimeOffset.UtcNow.ToOffset(ZonaBogota);

        string[] nombresAgentes =
        [
            "Agente Prueba Uno",
            "Agente Prueba Dos",
            "Agente Prueba Tres",
            "Agente Prueba Cuatro",
            "Agente Prueba Cinco"
        ];

        for (var i = 0; i < nombresAgentes.Length; i++)
        {
            yield return new Usuario
            {
                IdUsuario = $"AG-0{i + 1}",
                Nombre = nombresAgentes[i],
                Rol = RolUsuario.Agente,
                EstadoAgente = Dominio.EstadoAgente.Desconectado,
                EnEstadoDesde = ahora
            };
        }

        yield return new Usuario
        {
            IdUsuario = "SUP-01",
            Nombre = "Supervisor de Prueba",
            Rol = RolUsuario.Supervisor
        };

        yield return new Usuario
        {
            IdUsuario = "ADM-01",
            Nombre = "Administrador de Prueba Uno",
            Rol = RolUsuario.Administrador
        };

        yield return new Usuario
        {
            IdUsuario = "ADM-02",
            Nombre = "Administrador de Prueba Dos",
            Rol = RolUsuario.Administrador
        };
    }

    /// <summary>
    /// Nivel 1 tomado de la tipificacion de septiembre de 2026.
    /// Nivel 2 PROVISIONAL: esta por validar con el supervisor de la linea.
    /// </summary>
    private static IEnumerable<Tipificacion> ConstruirTipificaciones()
    {
        (string Nivel1, string? Nivel2, bool ExigeCita)[] catalogo =
        [
            ("ASIGNACION", "CITA ASIGNADA", true),
            ("ASIGNACION", "SIN CUPO DISPONIBLE", false),
            ("INFORMACION", "REQUISITOS DEL EXAMEN", false),
            ("INFORMACION", "HORARIOS Y SEDES", false),
            ("ESCALAMIENTO - NO CITA", "LUMIER NO DISPONIBLE", false),
            ("ESCALAMIENTO - NO CITA", "DERECHOS NO VIGENTES", false),
            ("CANCELACION", "CANCELA EL PACIENTE", false),
            ("CONFIRMACION", "CONFIRMA ASISTENCIA", false),
            ("PQR", "QUEJA POR ATENCION", false),
            ("NO CONTACTO", "LLAMADA CAIDA", false),
            ("OTROS", null, false)
        ];

        return catalogo.Select(c => new Tipificacion
        {
            Nivel1 = c.Nivel1,
            Nivel2 = c.Nivel2,
            ExigeCita = c.ExigeCita,
            Activa = true
        });
    }

    /// <summary>
    /// 30 pacientes inventados: 24 con derechos vigentes, 4 sin derechos
    /// y 2 con correo o telefono alternativo vacio.
    /// Los documentos 1000000098 y 1000000099 NO existen a proposito,
    /// para poder probar el error de "Paciente no encontrado".
    /// </summary>
    private static IEnumerable<PacienteEverest> ConstruirPacientes()
    {
        string[] nombres =
        [
            "María Fernanda Ruiz Pérez", "Carlos Andrés Gómez Loaiza", "Luz Dary Moreno Castaño",
            "Juan Sebastián Ramírez Ortiz", "Gloria Patricia Vargas Niño", "Andrés Felipe Quintero Salas",
            "Diana Carolina Hoyos Mejía", "Jorge Enrique Castillo Parra", "Sandra Milena Ospina Tobón",
            "Óscar Iván Beltrán Cárdenas", "Claudia Marcela Zuluaga Rendón", "Fabián Alberto Cortés Niño",
            "Yolanda Esperanza Rojas Pineda", "Mauricio Alejandro Benítez Correa", "Paula Andrea Giraldo Ocampo",
            "Héctor Guillermo Sánchez Arias", "Mónica Liliana Fajardo Peña", "Ricardo Antonio Lozano Gil",
            "Angélica María Duarte Serna", "Néstor Fabio Agudelo Marín", "Beatriz Elena Cárdenas Toro",
            "Wilson Javier Trujillo Bravo", "Adriana Lucía Pardo Estrada", "Germán Eduardo Salazar Mesa",
            "Martha Cecilia Bermúdez Rico", "Álvaro Hernán Cifuentes Gaitán", "Nubia Stella Carvajal Durán",
            "Edison Camilo Amaya Puentes", "Liliana Andrea Montoya Céspedes", "Rubén Darío Villamil Acosta"
        ];

        string[] eps =
        [
            "Nueva EPS", "EPS Sura", "EPS Sanitas", "Compensar EPS",
            "Salud Total EPS", "Coosalud EPS", "Famisanar EPS", "Aliansalud EPS"
        ];

        string[] sedes = [Sedes.Norte, Sedes.Sur];

        // Los 4 ultimos (indices 26 a 29) quedan sin derechos vigentes.
        const int primerIndiceSinDerechos = 26;

        // Los indices 24 y 25 quedan con un dato de contacto vacio.
        const int indiceSinCorreo = 24;
        const int indiceSinTelefono = 25;

        for (var i = 0; i < nombres.Length; i++)
        {
            var documento = $"10000000{(i + 1):D2}";
            var sinDerechos = i >= primerIndiceSinDerechos;

            yield return new PacienteEverest
            {
                TipoDocumento = "CC",
                NumeroDocumento = documento,
                NombreCompleto = nombres[i],
                Correo = i == indiceSinCorreo ? null : ConstruirCorreo(nombres[i], i),
                TelefonoAlterno = i == indiceSinTelefono ? null : $"310{(4000000 + i * 137):D7}",
                Sede = sedes[i % sedes.Length],
                Eps = eps[i % eps.Length],
                Derechos = sinDerechos ? EstadoDerechos.NoVigentes : EstadoDerechos.Vigentes
            };
        }
    }

    private static string ConstruirCorreo(string nombreCompleto, int indice)
    {
        var partes = nombreCompleto.Split(' ');
        var inicial = SinTildes(partes[0][..1]).ToLowerInvariant();
        var apellido = SinTildes(partes[^2]).ToLowerInvariant();
        return $"{inicial}{apellido}{indice + 1}@correo.com";
    }

    private static string SinTildes(string texto)
    {
        var normalizado = texto.Normalize(System.Text.NormalizationForm.FormD);
        var limpio = normalizado.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark);
        return new string(limpio.ToArray()).Normalize(System.Text.NormalizationForm.FormC);
    }

    /// <summary>
    /// Cupos de prueba para 3 examenes y 2 sedes durante las 2 semanas siguientes.
    /// </summary>
    private static IEnumerable<CupoLumier> ConstruirCupos()
    {
        (string Examen, string Cups)[] examenes =
        [
            ("Ecografía abdominal total", "881302"),
            ("Radiografía de tórax", "871121"),
            ("Mamografía bilateral", "876803")
        ];

        string[] sedes = [Sedes.Norte, Sedes.Sur];
        int[] horas = [7, 9, 11, 14, 16];

        var hoy = DateTimeOffset.UtcNow.ToOffset(ZonaBogota).Date;

        for (var dia = 1; dia <= 14; dia++)
        {
            var fecha = hoy.AddDays(dia);

            // Sin cupos los domingos.
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                continue;
            }

            foreach (var (examen, cups) in examenes)
            {
                foreach (var sede in sedes)
                {
                    foreach (var hora in horas)
                    {
                        yield return new CupoLumier
                        {
                            Examen = examen,
                            CodigoCups = cups,
                            Sede = sede,
                            FechaHora = new DateTimeOffset(
                                fecha.AddHours(hora).AddMinutes(sede == Sedes.Norte ? 0 : 40),
                                ZonaBogota)
                        };
                    }
                }
            }
        }
    }
}

public static class Sedes
{
    public const string Norte = "Sede Norte";
    public const string Sur = "Sede Sur";
}
