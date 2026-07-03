using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Zoapex.Web.Data.Models;

namespace Zoapex.Web.Data;

public class OrderRepository(ZoapexDbContext ctx)
{
    // Procedimiento almacenado (función PostgreSQL): registro transaccional maestro-detalle
    public async Task<int> RegisterOrderAsync(int? customerId, IReadOnlyList<CartLineDto> lines)
    {
        if (lines.Count == 0)
            throw new InvalidOperationException("El carrito no puede estar vacío.");

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var stock = await ctx.Products
                .AsNoTracking()
                .Where(p => p.ProductId == line.ProductId && p.Status == 1)
                .Select(p => p.Stock)
                .FirstOrDefaultAsync();

            if (stock < line.Quantity)
                throw new InvalidOperationException(
                    $"Stock insuficiente para '{line.ProductName}'. Disponible: {stock}.");
        }

        var payload = lines.Select(l => new
        {
            product_id = l.ProductId,
            quantity = l.Quantity,
            unit_price = l.UnitPrice
        });

        var json = JsonSerializer.Serialize(payload);

        var orderId = await ctx.Database
            .SqlQuery<int>($"SELECT fn_register_order({customerId}, {json}::jsonb) AS \"Value\"")
            .SingleAsync();

        return orderId;
    }

    // LINQ: historial del cliente autenticado
    public async Task<List<OrderHistoryDto>> GetCustomerOrdersAsync(int customerId, int take = 20)
    {
        return await ctx.Orders
            .AsNoTracking()
            .Include(o => o.Details)
            .ThenInclude(d => d.Product)
            .Where(o => o.Status == 1 && o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .Take(take)
            .Select(o => new OrderHistoryDto(
                o.OrderId,
                o.Code,
                o.OrderDate,
                o.Subtotal,
                o.Tax,
                o.Total,
                o.Details
                    .Select(d => new OrderDetailLineDto(
                        d.Product.Name,
                        d.Quantity,
                        d.UnitPrice,
                        d.Subtotal))
                    .ToList()))
            .ToListAsync();
    }

    // LINQ: historial general (admin/demo)
    public async Task<List<OrderHistoryDto>> GetRecentOrdersAsync(int take = 20)
    {
        return await ctx.Orders
            .AsNoTracking()
            .Include(o => o.Details)
            .ThenInclude(d => d.Product)
            .Where(o => o.Status == 1)
            .OrderByDescending(o => o.OrderDate)
            .Take(take)
            .Select(o => new OrderHistoryDto(
                o.OrderId,
                o.Code,
                o.OrderDate,
                o.Subtotal,
                o.Tax,
                o.Total,
                o.Details
                    .Select(d => new OrderDetailLineDto(
                        d.Product.Name,
                        d.Quantity,
                        d.UnitPrice,
                        d.Subtotal))
                    .ToList()))
            .ToListAsync();
    }

    public static decimal CalculateSubtotal(IEnumerable<CartLineDto> lines)
        => lines.Sum(l => l.Subtotal);

    public static decimal CalculateTax(decimal subtotal)
        => Math.Round(subtotal * 0.18m, 2);

    public static decimal CalculateTotal(decimal subtotal)
        => subtotal + CalculateTax(subtotal);
}
