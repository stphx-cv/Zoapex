using System.Text.Json;
using Npgsql;
using Zoapex.Entities;

namespace Zoapex.DataAccess;

public class OrderDAL
{
    private readonly DatabaseConnection _connection = new();

    // Registra el pedido completo usando la función PostgreSQL con JSONB
    public int RegisterOrder(int customerId, List<OrderDetailEntity> details)
    {
        var detailsJson = JsonSerializer.Serialize(details.Select(d => new
        {
            product_id = d.ProductId,
            quantity   = d.Quantity,
            unit_price = d.UnitPrice
        }));

        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand(
            "SELECT fn_register_order(@p_customer_id, @p_details::jsonb)", cnx);
        cmd.Parameters.AddWithValue("p_customer_id", customerId);
        cmd.Parameters.AddWithValue("p_details",     detailsJson);
        cnx.Open();
        var result = cmd.ExecuteScalar();
        return result == null ? 0 : Convert.ToInt32(result);
    }
}
