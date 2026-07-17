using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Zoapex.Web.Data.Models;

namespace Zoapex.Web.Data;

// Construye y lee la identidad del usuario a partir de los claims de la cookie
// de autenticación (reemplaza al antiguo esquema basado en ISession).
public static class CustomerIdentity
{
    // Clave del claim propio con el Id del cliente (no existe un ClaimType estándar).
    public const string CustomerIdClaim = "CustomerId";

    // Arma el ClaimsPrincipal que se firma en la cookie: nombre, correo, rol e Id.
    public static ClaimsPrincipal Build(Customer customer)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, customer.FullName),
            new(ClaimTypes.Email, customer.Email ?? string.Empty),
            new(ClaimTypes.Role, customer.Role),              // un claim de rol (Admin / Cliente)
            new(CustomerIdClaim, customer.CustomerId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    // Devuelve el Id del cliente autenticado (o null si no ha iniciado sesión).
    public static int? GetCustomerId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(CustomerIdClaim);
        return int.TryParse(value, out var id) ? id : null;
    }

    // Verdadero si el usuario tiene el rol de administrador.
    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin");
}
