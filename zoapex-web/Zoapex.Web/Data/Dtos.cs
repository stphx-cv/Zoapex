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
