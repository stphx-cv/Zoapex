using System.Data;
using Npgsql;
using Zoapex.Entities;

namespace Zoapex.DataAccess;

public class ProductDAL
{
    private readonly DatabaseConnection _connection = new();

    public DataTable GetAllProducts()
    {
        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand("SELECT * FROM fn_list_products()", cnx);
        var adapter = new NpgsqlDataAdapter(cmd);
        var table   = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public ProductEntity? GetProduct(int productId)
    {
        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand("SELECT * FROM fn_get_product(@p_id)", cnx);
        cmd.Parameters.AddWithValue("p_id", productId);
        cnx.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapProduct(reader) : null;
    }

    public void InsertProduct(ProductEntity product)
    {
        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand(
            "SELECT fn_insert_product(@p_name,@p_desc,@p_price,@p_stock,@p_min,@p_cat,@p_img)", cnx);
        cmd.Parameters.AddWithValue("p_name",  product.Name);
        cmd.Parameters.AddWithValue("p_desc",  product.Description);
        cmd.Parameters.AddWithValue("p_price", product.Price);
        cmd.Parameters.AddWithValue("p_stock", product.Stock);
        cmd.Parameters.AddWithValue("p_min",   product.MinStock);
        cmd.Parameters.AddWithValue("p_cat",   product.CategoryId);
        cmd.Parameters.AddWithValue("p_img",   product.ImageUrl);
        cnx.Open();
        cmd.ExecuteNonQuery();
    }

    public void UpdateProduct(ProductEntity product)
    {
        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand(
            "SELECT fn_update_product(@p_id,@p_name,@p_desc,@p_price,@p_stock,@p_min,@p_cat,@p_img)", cnx);
        cmd.Parameters.AddWithValue("p_id",    product.ProductId);
        cmd.Parameters.AddWithValue("p_name",  product.Name);
        cmd.Parameters.AddWithValue("p_desc",  product.Description);
        cmd.Parameters.AddWithValue("p_price", product.Price);
        cmd.Parameters.AddWithValue("p_stock", product.Stock);
        cmd.Parameters.AddWithValue("p_min",   product.MinStock);
        cmd.Parameters.AddWithValue("p_cat",   product.CategoryId);
        cmd.Parameters.AddWithValue("p_img",   product.ImageUrl);
        cnx.Open();
        cmd.ExecuteNonQuery();
    }

    public void DeleteProduct(int productId)
    {
        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand("SELECT fn_delete_product(@p_id)", cnx);
        cmd.Parameters.AddWithValue("p_id", productId);
        cnx.Open();
        cmd.ExecuteNonQuery();
    }

    private static ProductEntity MapProduct(NpgsqlDataReader r) => new()
    {
        ProductId    = r.GetInt32(r.GetOrdinal("product_id")),
        Code         = r.GetString(r.GetOrdinal("code")),
        Name         = r.GetString(r.GetOrdinal("name")),
        Description  = r.IsDBNull(r.GetOrdinal("description"))   ? string.Empty : r.GetString(r.GetOrdinal("description")),
        Price        = r.GetDecimal(r.GetOrdinal("price")),
        Stock        = r.GetInt32(r.GetOrdinal("stock")),
        MinStock     = r.GetInt32(r.GetOrdinal("min_stock")),
        CategoryId   = r.GetInt32(r.GetOrdinal("category_id")),
        CategoryName = r.IsDBNull(r.GetOrdinal("category_name")) ? string.Empty : r.GetString(r.GetOrdinal("category_name")),
        ImageUrl     = r.IsDBNull(r.GetOrdinal("image_url"))     ? string.Empty : r.GetString(r.GetOrdinal("image_url")),
        Status       = r.GetInt16(r.GetOrdinal("status")),
    };
}
