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

    public void OnGet()
    {
        if (CustomerSession.IsLoggedIn(HttpContext.Session))
            Response.Redirect("/Catalog");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var customer = await customerRepo.RegisterAsync(
                FirstName, LastName, Email, Password, Phone, Address);

            CustomerSession.SignIn(
                HttpContext.Session,
                customer.CustomerId,
                customer.FullName,
                customer.Email ?? Email,
                customer.Role);

            return RedirectToPage("/Catalog");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
