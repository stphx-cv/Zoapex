using Zoapex.DataAccess;
using Zoapex.Entities;

namespace Zoapex.Business;

public class OrderBL
{
    private readonly OrderDAL   _orderDal   = new();
    private readonly ProductDAL _productDal = new();

    public int RegisterOrder(int? customerId, List<OrderDetailEntity> details)
    {
        // El carrito no puede estar vacío
        if (details == null || details.Count == 0)
            throw new Exception("The cart cannot be empty.");

        // Valida cada ítem antes de registrar
        foreach (var item in details)
        {
            if (item.Quantity <= 0)
                throw new Exception($"Quantity for '{item.ProductName}' must be greater than zero.");

            var product = _productDal.GetProduct(item.ProductId)
                ?? throw new Exception($"Product ID {item.ProductId} was not found.");

            // Verifica stock suficiente
            if (product.Stock < item.Quantity)
                throw new Exception(
                    $"Insufficient stock for '{product.Name}'. Available: {product.Stock}, requested: {item.Quantity}.");
        }

        return _orderDal.RegisterOrder(customerId, details);
    }

    // Calcula el subtotal del carrito (suma de quantity × unit_price)
    public static decimal CalculateSubtotal(List<OrderDetailEntity> details)
        => details.Sum(d => d.Quantity * d.UnitPrice);

    // IGV peruano: 18%
    public static decimal CalculateTax(decimal subtotal)
        => Math.Round(subtotal * 0.18m, 2);

    public static decimal CalculateTotal(decimal subtotal)
        => subtotal + CalculateTax(subtotal);
}
