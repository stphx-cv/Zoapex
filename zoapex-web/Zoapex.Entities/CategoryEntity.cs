namespace Zoapex.Entities;

public class CategoryEntity
{
    public int    CategoryId  { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    Status      { get; set; }
}
