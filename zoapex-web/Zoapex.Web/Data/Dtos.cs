namespace Zoapex.Web.Data;

public record ProductCardDto(
    int ProductId,
    string Code,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string CategoryName);

public record ProductDetailDto(
    int ProductId,
    string Code,
    string Name,
    decimal Price,
    int Stock,
    string CategoryName);

public record CategoryOptionDto(int CategoryId, string Name);

public record CartLineDto(
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);

public record OrderSummaryDto(
    int OrderId,
    string Code,
    DateTime OrderDate,
    decimal Total,
    int ItemCount);

public record OrderDetailLineDto(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record OrderHistoryDto(
    int OrderId,
    string Code,
    DateTime OrderDate,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    IReadOnlyList<OrderDetailLineDto> Details);

// ---- Reporte de ventas (Examen Final: mostrar + exportar) ----

public record TopProductDto(string ProductName, int Quantity, decimal Revenue);

public record SalesByDateDto(DateTime Date, int Orders, decimal Total);

public record SalesOrderRowDto(
    string Code,
    DateTime OrderDate,
    string CustomerName,
    int Items,
    decimal Subtotal,
    decimal Tax,
    decimal Total);

public record SalesReportDto(
    int OrderCount,
    decimal TotalRevenue,
    decimal AverageTicket,
    int UnitsSold,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<SalesByDateDto> SalesByDate,
    IReadOnlyList<SalesOrderRowDto> Orders);
