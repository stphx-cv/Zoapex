using Microsoft.EntityFrameworkCore;
using Zoapex.Web.Data.Models;

namespace Zoapex.Web.Data;

public static class DbSeeder
{
    public static async Task EnsureSchemaAndDemoUserAsync(ZoapexDbContext ctx)
    {
        // Agrega columnas si aún no existen en Supabase (evolución idempotente del esquema)
        await ctx.Database.ExecuteSqlRawAsync(
            "ALTER TABLE customer ADD COLUMN IF NOT EXISTS password_hash VARCHAR(512)");
        await ctx.Database.ExecuteSqlRawAsync(
            "ALTER TABLE customer ADD COLUMN IF NOT EXISTS role VARCHAR(20) NOT NULL DEFAULT 'Cliente'");

        const string demoEmail = "cliente@zoapex.com";
        if (!await ctx.Customers.AnyAsync(c => c.Email == demoEmail))
        {
            ctx.Customers.Add(new Customer
            {
                FirstName = "María",
                LastName = "López",
                Email = demoEmail,
                Phone = "999888777",
                Address = "Av. Larco 123, Miraflores",
                PasswordHash = PasswordHelper.Hash("zoapex123"),
                Role = "Cliente",
                Status = 1
            });
        }

        const string adminEmail = "admin@zoapex.com";
        if (!await ctx.Customers.AnyAsync(c => c.Email == adminEmail))
        {
            ctx.Customers.Add(new Customer
            {
                FirstName = "Admin",
                LastName = "Zoapex",
                Email = adminEmail,
                Phone = "999000111",
                Address = "Sede central Zoapex",
                PasswordHash = PasswordHelper.Hash("zoapex123"),
                Role = "Admin",
                Status = 1
            });
        }

        await ctx.SaveChangesAsync();
    }
}
