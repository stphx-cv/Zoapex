using System.Data;
using Npgsql;

namespace Zoapex.DataAccess;

public class CategoryDAL
{
    private readonly DatabaseConnection _connection = new();

    public DataTable GetAllCategories()
    {
        using var cnx = new NpgsqlConnection(_connection.GetConnectionString());
        using var cmd = new NpgsqlCommand("SELECT * FROM fn_list_categories()", cnx);
        var adapter = new NpgsqlDataAdapter(cmd);
        var table   = new DataTable();
        adapter.Fill(table);
        return table;
    }
}
