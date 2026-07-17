using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages;

public class OrdersModel(OrderRepository orderRepo) : PageModel
{
    public List<OrderHistoryDto> Orders { get; private set; } = [];
    public string? CustomerName { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // La página exige autenticación (convención en Program.cs); igual validamos el claim
        var customerId = User.GetCustomerId();
        if (customerId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = "/Orders" });

        CustomerName = User.Identity?.Name;
        Orders = await orderRepo.GetCustomerOrdersAsync(customerId.Value);
        return Page();
    }
}
