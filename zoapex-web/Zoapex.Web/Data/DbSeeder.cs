using Microsoft.EntityFrameworkCore;
using Zoapex.Web.Data.Models;

namespace Zoapex.Web.Data;

public static class DbSeeder
{
    public static async Task EnsureSchemaAndDemoUserAsync(ZoapexDbContext ctx)
    {
        // Agrega la columna de contraseña si aún no existe en Supabase
        await ctx.Database.ExecuteSqlRawAsync(
            "ALTER TABLE customer ADD COLUMN IF NOT EXISTS password_hash VARCHAR(512)");

        const string demoEmail = "cliente@zoapex.com";
        var exists = await ctx.Customers.AnyAsync(c => c.Email == demoEmail);
        if (exists)
            return;

        ctx.Customers.Add(new Customer
        {
            FirstName = "María",
            LastName = "López",
            Email = demoEmail,
            Phone = "999888777",
            Address = "Av. Larco 123, Miraflores",
            PasswordHash = PasswordHelper.Hash("zoapex123"),
            Status = 1
        });

        await ctx.SaveChangesAsync();
    }
}
