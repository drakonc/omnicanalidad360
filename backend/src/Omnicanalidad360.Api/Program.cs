using Microsoft.EntityFrameworkCore;
using Omnicanalidad360.Api.Datos;
using Omnicanalidad360.Api.Dominio;
using Omnicanalidad360.Api.Endpoints;
using Omnicanalidad360.Api.Simulaciones.Telefonia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Base de datos local en SQLite, al lado del proyecto.
var rutaBaseDatos = Path.Combine(builder.Environment.ContentRootPath, "omnicanalidad360.db");
builder.Services.AddDbContext<ContextoOmnicanalidad>(opciones =>
    opciones.UseSqlite($"Data Source={rutaBaseDatos}"));

builder.Services.AddScoped<ServicioLlamadas>();

// El simulador de la llamada se registra por separado: el dia que llegue la
// telefonia de Azure se cambia esta sola linea y la consola no se toca.
builder.Services.AddScoped<ITelefonia, SimuladorTelefonia>();

// La pantalla de Angular corre en otro puerto del mismo computador.
const string PoliticaPantalla = "pantalla-local";
builder.Services.AddCors(opciones =>
    opciones.AddPolicy(PoliticaPantalla, politica => politica
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// Crea la base si no existe y carga los datos de prueba del PRD.
using (var alcance = app.Services.CreateScope())
{
    var contexto = alcance.ServiceProvider.GetRequiredService<ContextoOmnicanalidad>();
    contexto.Database.EnsureCreated();
    DatosSemilla.Cargar(contexto);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(PoliticaPantalla);

app.MapGet("/", () => Results.Ok(new
{
    plataforma = "Omnicanalidad 360",
    linea = Lineas.NepsImagenologia,
    nota = "Version local con la llamada, Everest y LUMIER simulados."
}));

app.MapearEndpointsConsola();
app.MapearEndpointsSupervision();

app.Run();
