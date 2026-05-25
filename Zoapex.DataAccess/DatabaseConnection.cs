using Microsoft.Extensions.Configuration;

namespace Zoapex.DataAccess;

public class DatabaseConnection
{
    public string GetConnectionString()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return config.GetConnectionString("ZoapexDb") ?? string.Empty;
    }
}
