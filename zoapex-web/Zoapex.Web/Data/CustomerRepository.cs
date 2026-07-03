using Microsoft.EntityFrameworkCore;
using Zoapex.Web.Data.Models;

namespace Zoapex.Web.Data;

public class CustomerRepository(ZoapexDbContext ctx)
{
    // LINQ: valida credenciales del cliente
    public async Task<Customer?> ValidateLoginAsync(string email, string password)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var customer = await ctx.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Email != null &&
                c.Email.ToLower() == normalized &&
                c.Status == 1);

        if (customer is null || string.IsNullOrEmpty(customer.PasswordHash))
            return null;

        return PasswordHelper.Verify(customer.PasswordHash, password) ? customer : null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await ctx.Customers.AnyAsync(c =>
            c.Email != null && c.Email.ToLower() == normalized);
    }

    public async Task<Customer> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string? phone,
        string? address)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidOperationException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidOperationException("El apellido es obligatorio.");
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("El correo es obligatorio.");
        if (password.Length < 6)
            throw new InvalidOperationException("La contraseña debe tener al menos 6 caracteres.");

        if (await EmailExistsAsync(email))
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");

        var customer = new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim(),
            Address = address?.Trim(),
            PasswordHash = PasswordHelper.Hash(password),
            Status = 1
        };

        ctx.Customers.Add(customer);
        await ctx.SaveChangesAsync();
        return customer;
    }
}
