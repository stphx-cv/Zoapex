using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        CustomerSession.SignOut(HttpContext.Session);
        CartSession.Clear(HttpContext.Session);
        return RedirectToPage("/Index");
    }
}
