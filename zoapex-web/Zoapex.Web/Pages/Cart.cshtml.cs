using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages;

[IgnoreAntiforgeryToken]
public class CartModel(CatalogRepository catalogRepo, OrderRepository orderRepo) : PageModel
{
    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }
    public List<CartLineDto> Lines { get; private set; } = [];

    public bool IsLoggedIn { get; private set; }
    public string? CustomerName { get; private set; }

    public void OnGet()
    {
        IsLoggedIn = User.Identity?.IsAuthenticated == true;
        CustomerName = User.Identity?.Name;
        RefreshTotals();
    }

    // AJAX: agrega producto al carrito desde el catálogo
    public async Task<IActionResult> OnPostAddAsync([FromBody] AddToCartRequest request)
    {
        if (request.Quantity <= 0)
            return new JsonResult(new { ok = false, message = "La cantidad debe ser mayor a cero." });

        var product = await catalogRepo.GetProductDetailAsync(request.ProductId);
        if (product is null)
            return new JsonResult(new { ok = false, message = "Producto no encontrado." });

        if (request.Quantity > product.Stock)
            return new JsonResult(new { ok = false, message = $"Stock insuficiente. Disponible: {product.Stock}." });

        var lines = CartSession.GetLines(HttpContext.Session);
        var existing = lines.FirstOrDefault(l => l.ProductId == product.ProductId);

        if (existing is not null)
        {
            var newQty = existing.Quantity + request.Quantity;
            if (newQty > product.Stock)
                return new JsonResult(new { ok = false, message = $"Stock insuficiente. Disponible: {product.Stock}." });

            existing = existing with
            {
                Quantity = newQty,
                Subtotal = newQty * existing.UnitPrice
            };
            lines.RemoveAll(l => l.ProductId == product.ProductId);
            lines.Add(existing);
        }
        else
        {
            lines.Add(new CartLineDto(
                product.ProductId,
                product.Name,
                product.Price,
                request.Quantity,
                product.Price * request.Quantity));
        }

        CartSession.SaveLines(HttpContext.Session, lines);
        return new JsonResult(new { ok = true, message = $"'{product.Name}' agregado al carrito.", count = lines.Count });
    }

    // AJAX: devuelve resumen del carrito sin recargar
    public IActionResult OnGetSummaryAsync()
    {
        var lines = CartSession.GetLines(HttpContext.Session);
        var subtotal = OrderRepository.CalculateSubtotal(lines);
        var tax = OrderRepository.CalculateTax(subtotal);
        var total = OrderRepository.CalculateTotal(subtotal);

        return new JsonResult(new
        {
            lines,
            subtotal,
            tax,
            total,
            count = lines.Count
        });
    }

    // AJAX: quita una línea del carrito
    public IActionResult OnPostRemoveAsync([FromBody] RemoveFromCartRequest request)
    {
        var lines = CartSession.GetLines(HttpContext.Session);
        lines.RemoveAll(l => l.ProductId == request.ProductId);
        CartSession.SaveLines(HttpContext.Session, lines);
        return OnGetSummaryAsync();
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        var customerId = User.GetCustomerId();
        if (customerId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = "/Cart" });

        var lines = CartSession.GetLines(HttpContext.Session);
        if (lines.Count == 0)
        {
            TempData["Error"] = "El carrito está vacío.";
            return RedirectToPage();
        }

        try
        {
            var orderId = await orderRepo.RegisterOrderAsync(customerId, lines);
            CartSession.Clear(HttpContext.Session);
            TempData["Success"] = $"¡Pedido registrado correctamente! Número de orden: {orderId}";
            return RedirectToPage("/Orders");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage();
        }
    }

    private void RefreshTotals()
    {
        Lines = CartSession.GetLines(HttpContext.Session);
        Subtotal = OrderRepository.CalculateSubtotal(Lines);
        Tax = OrderRepository.CalculateTax(Subtotal);
        Total = OrderRepository.CalculateTotal(Subtotal);
    }

    public record AddToCartRequest(int ProductId, int Quantity);
    public record RemoveFromCartRequest(int ProductId);
}
