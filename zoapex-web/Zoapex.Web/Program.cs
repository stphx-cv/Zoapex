using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<Zoapex.Web.Data.ZoapexDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ZoapexDb"),
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));
builder.Services.AddScoped<Zoapex.Web.Data.CatalogRepository>();
builder.Services.AddScoped<Zoapex.Web.Data.OrderRepository>();
builder.Services.AddScoped<Zoapex.Web.Data.CustomerRepository>();
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
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
