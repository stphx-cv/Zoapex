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

    public void OnGet(string? returnUrl)
    {
        ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/Catalog" : returnUrl;

        if (CustomerSession.IsLoggedIn(HttpContext.Session))
            Response.Redirect(ReturnUrl);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/Catalog" : returnUrl;

        var customer = await customerRepo.ValidateLoginAsync(Email, Password);
        if (customer is null)
        {
            ErrorMessage = "Correo o contraseña incorrectos.";
            return Page();
        }

        CustomerSession.SignIn(
            HttpContext.Session,
            customer.CustomerId,
            customer.FullName,
            customer.Email ?? Email);

        return Redirect(ReturnUrl);
    }
}
