using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages.Account;

public class RegisterModel(CustomerRepository customerRepo) : PageModel
{
    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string Phone { get; set; } = string.Empty;

    [BindProperty]
    public string Address { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Catalog");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var customer = await customerRepo.RegisterAsync(
                FirstName, LastName, Email, Password, Phone, Address);

            // Inicia sesión automáticamente firmando la cookie con sus claims
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                CustomerIdentity.Build(customer),
                new AuthenticationProperties { IsPersistent = false });

            return RedirectToPage("/Catalog");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
