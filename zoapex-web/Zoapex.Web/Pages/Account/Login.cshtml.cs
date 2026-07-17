using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages.Account;

public class LoginModel(CustomerRepository customerRepo) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? returnUrl)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);

        // Si ya inició sesión, no tiene sentido mostrar el login
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(ReturnUrl);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);

        var customer = await customerRepo.ValidateLoginAsync(Email, Password);
        if (customer is null)
        {
            ErrorMessage = "Correo o contraseña incorrectos.";
            return Page();
        }

        // Firma la cookie de autenticación con el nombre, correo, rol e Id del cliente
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            CustomerIdentity.Build(customer),
            new AuthenticationProperties { IsPersistent = false });

        return LocalRedirect(ReturnUrl);
    }

    // Evita open-redirect: solo se permiten rutas locales
    private string SafeReturnUrl(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/Catalog";
}
