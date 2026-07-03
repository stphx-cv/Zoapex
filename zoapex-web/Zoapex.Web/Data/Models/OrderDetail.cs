using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoapex.Web.Data.Models;

[Table("order_detail")]
public class OrderDetail
{
    [Key]
    [Column("detail_id")]
    public int DetailId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
