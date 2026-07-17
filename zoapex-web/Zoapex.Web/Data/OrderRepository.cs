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

    // LINQ: reporte de ventas con análisis (KPIs, top productos, ventas por fecha, detalle)
    // Es la "query adjunta" que alimenta el reporte web y la descarga a Excel.
    public async Task<SalesReportDto> GetSalesReportAsync()
    {
        // Filas de pedidos (cabecera + nombre de cliente + n° de ítems)
        var orders = await ctx.Orders
            .AsNoTracking()
            .Where(o => o.Status == 1)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new SalesOrderRowDto(
                o.Code,
                o.OrderDate,
                o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : "Mostrador",
                o.Details.Sum(d => (int?)d.Quantity) ?? 0,
                o.Subtotal,
                o.Tax,
                o.Total))
            .ToListAsync();

        // Detalle plano (traducible a SQL) y agrupación en memoria por producto
        var detailRows = await ctx.OrderDetails
            .AsNoTracking()
            .Where(d => d.Order.Status == 1)
            .Select(d => new { d.Product.Name, d.Quantity, d.Subtotal })
            .ToListAsync();

        var topProducts = detailRows
            .GroupBy(d => d.Name)
            .Select(g => new TopProductDto(
                g.Key,
                g.Sum(d => d.Quantity),
                g.Sum(d => d.Subtotal)))
            .OrderByDescending(t => t.Revenue)
            .Take(5)
            .ToList();

        // Ventas por fecha, calculadas desde los pedidos ya materializados
        var salesByDate = orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new SalesByDateDto(
                g.Key,
                g.Count(),
                g.Sum(o => o.Total)))
            .OrderBy(s => s.Date)
            .ToList();

        var orderCount = orders.Count;
        var totalRevenue = orders.Sum(o => o.Total);
        var unitsSold = orders.Sum(o => o.Items);
        var averageTicket = orderCount > 0 ? Math.Round(totalRevenue / orderCount, 2) : 0m;

        return new SalesReportDto(
            orderCount,
            totalRevenue,
            averageTicket,
            unitsSold,
            topProducts,
            salesByDate,
            orders);
    }

    public static decimal CalculateSubtotal(IEnumerable<CartLineDto> lines)
        => lines.Sum(l => l.Subtotal);

    public static decimal CalculateTax(decimal subtotal)
        => Math.Round(subtotal * 0.18m, 2);

    public static decimal CalculateTotal(decimal subtotal)
        => subtotal + CalculateTax(subtotal);
}
