using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Licencia EPPlus (uso académico / no comercial)
ExcelPackage.License.SetNonCommercialPersonal("Stephano Camarena Villa");

// Minutos de caducidad de la sesión (configurables en appsettings.json)
var timeoutMin = builder.Configuration.GetValue("Security:SessionTimeoutMinutes", 30);

// Autenticación por cookie (reemplaza al esquema basado en ISession)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(timeoutMin);
        options.SlidingExpiration = true;   // la cookie se renueva con la actividad
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// Políticas de autorización por rol (reemplazan al roleManager del stack antiguo)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsAdmin", policy => policy.RequireRole("Admin"));
});

// Reglas de acceso a las páginas del sitio (convenciones de Razor Pages).
// El resto de páginas queda anónimo (catálogo y carrito son públicos).
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Orders");              // requiere haber iniciado sesión
    options.Conventions.AuthorizePage("/SalesReport", "EsAdmin"); // solo administrador
});
builder.Services.AddDbContext<Zoapex.Web.Data.ZoapexDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ZoapexDb"),
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));
builder.Services.AddScoped<Zoapex.Web.Data.CatalogRepository>();
builder.Services.AddScoped<Zoapex.Web.Data.OrderRepository>();
builder.Services.AddScoped<Zoapex.Web.Data.CustomerRepository>();
builder.Services.AddScoped<Zoapex.Web.Data.SalesExcelExporter>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Zoapex.Web.Data.ZoapexDbContext>();
    try
    {
        await Zoapex.Web.Data.DbSeeder.EnsureSchemaAndDemoUserAsync(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo inicializar la base de datos. Revisa la cadena de conexión en appsettings.json.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();          // el carrito de invitado sigue viviendo en la sesión
app.UseAuthentication();   // primero se autentica (quién es)
app.UseAuthorization();    // luego se autoriza (qué puede hacer)
app.MapRazorPages();

app.Run();
