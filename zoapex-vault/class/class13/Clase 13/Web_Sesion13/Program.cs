using Microsoft.EntityFrameworkCore;
using Web_Sesion13.Datos;
using Web_Sesion13.Modelos;
using Microsoft.Extensions.Logging;

// Punto de entrada de la aplicacion. Aqui se registran los servicios
// y se arma la tuberia de peticiones (middlewares).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// El AJAX de guardado (POST) envia el token antiforgery en una cabecera.
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

// Se registra el DbContext de EF Core con la cadena de conexion de appsettings.json.
// A partir de aqui, cualquier pagina o clase puede recibir el contexto por inyeccion.
builder.Services.AddDbContext<VentasLeonContext>(opciones =>
    //opciones.UseSqlServer(builder.Configuration.GetConnectionString("VentasLeon")));
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("VentasLeon"))
            .LogTo(Console.WriteLine, LogLevel.Information));

// La capa de datos: expone los metodos de consulta (LINQ y procedimientos almacenados).
builder.Services.AddScoped<VentasRepositorio>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // permite servir css y js desde wwwroot
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
