using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoapex.Web.Data.Models;

[Table("product")]
public class Product
{
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("min_stock")]
    public int MinStock { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("status")]
    public short Status { get; set; }

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; }

    public Category Category { get; set; } = null!;
}
