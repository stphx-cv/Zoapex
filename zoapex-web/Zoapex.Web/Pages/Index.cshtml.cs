using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zoapex.Web.Pages;

public class IndexModel : PageModel
{
    public string Author { get; set; } = "Stephano Camarena Villa";

    public void OnGet()
    {
    }
}
