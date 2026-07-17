using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages.Account;

public class LogoutModel : PageModel
{
    // POST: cierre de sesión desde el botón "Salir"
    public async Task<IActionResult> OnPostAsync() => await SignOutAndRedirectAsync();

    // GET: cierre de sesión por caducidad (el JS de inactividad redirige aquí)
    public async Task<IActionResult> OnGetAsync() => await SignOutAndRedirectAsync();

    private async Task<IActionResult> SignOutAndRedirectAsync()
    {
        // Borra la cookie de autenticación y vacía el carrito de la sesión
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        CartSession.Clear(HttpContext.Session);
        return RedirectToPage("/Account/Login");
    }
}
