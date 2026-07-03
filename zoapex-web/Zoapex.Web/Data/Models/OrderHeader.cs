using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoapex.Web.Data.Models;

[Table("order")]
public class OrderHeader
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("order_date")]
    public DateTime OrderDate { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("tax")]
    public decimal Tax { get; set; }

    [Column("total")]
    public decimal Total { get; set; }

    [Column("status")]
    public short Status { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<OrderDetail> Details { get; set; } = [];
}
