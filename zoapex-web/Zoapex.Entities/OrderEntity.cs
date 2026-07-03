namespace Zoapex.Entities;

public class OrderEntity
{
    public int      OrderId    { get; set; }
    public string   Code       { get; set; } = string.Empty;
    public int      CustomerId { get; set; }
    public DateTime OrderDate  { get; set; }
    public decimal  Subtotal   { get; set; }
    public decimal  Tax        { get; set; }
    public decimal  Total      { get; set; }
    public int      Status     { get; set; }
}
