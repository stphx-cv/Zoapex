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
        var customerId = CustomerSession.GetCustomerId(HttpContext.Session);
        if (customerId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = "/Orders" });

        CustomerName = CustomerSession.GetCustomerName(HttpContext.Session);
        Orders = await orderRepo.GetCustomerOrdersAsync(customerId.Value);
        return Page();
    }
}
