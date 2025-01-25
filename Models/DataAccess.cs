using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FlashcardSimulator;

public class DataAccess
{
    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    private string? ConnectionString;

    public DataAccess()
    {
        ConnectionString = config.GetSection("ConnectionStrings")["DefaultConnection"];
    }

    public bool TestConnection()
    {
        try
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var result = connection.QuerySingle<int>("SELECT 1");
                return result == 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return false;
        }
    }
}
