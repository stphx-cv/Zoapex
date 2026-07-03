namespace Zoapex.Entities;

public class ProductEntity
{
    public int      ProductId    { get; set; }
    public string   Code         { get; set; } = string.Empty;
    public string   Name         { get; set; } = string.Empty;
    public string   Description  { get; set; } = string.Empty;
    public decimal  Price        { get; set; }
    public int      Stock        { get; set; }
    public int      MinStock     { get; set; }
    public int      CategoryId   { get; set; }
    public string   CategoryName { get; set; } = string.Empty;
    public string   ImageUrl     { get; set; } = string.Empty;
    public int      Status       { get; set; }
    public DateTime RegisteredAt { get; set; }
}
