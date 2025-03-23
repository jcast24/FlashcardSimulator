using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FlashcardSimulator.Models;

public class DataAccess
{
    private readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    private readonly string? _connectionString;

    public DataAccess()
    {
        _connectionString = _config.GetSection("ConnectionStrings")["DefaultConnection"];
    }

    public string? GetConnection()
    {
        return _connectionString;
    }

    internal void CreateTables()
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string createStackTableSql =
                    @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stacks')
                    CREATE TABLE Stacks (
                            Id int IDENTITY(1,1) NOT NULL,
                            Name NVARCHAR(30) NOT NULL UNIQUE,
                            PRIMARY KEY (Id)
                            );
                ";
                conn.Execute(createStackTableSql);

                string createFlashCardTableSql =
                    @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Flashcards')
                    CREATE TABLE Flashcards (
                        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Question NVARCHAR(30) NOT NULL,
                        Answer NVARCHAR(30) NOT NULL,
                        StackId int NOT NULL
                            FOREIGN KEY
                            REFERENCES Stacks(Id)
                            ON DELETE CASCADE
                            ON UPDATE CASCADE
                    );";

                conn.Execute(createFlashCardTableSql);

                string createFlashcardViewTable =
                    @"
                    CREATE VIEW FlashcardView AS 
                    SELECT
                        f.Id AS FlashcardId,
                        f.Question,
                        f.Answer,
                        f.StackId,
                        s.Name AS StackName
                    From Flashcards f 
                    JOIN Stacks s ON f.StackId = s.Id;
                    ";
                conn.Execute(createFlashcardViewTable);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

}
