using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace FlashcardSimulator;

public class DataAccess
{
    IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    private string? ConnectionString;

    public DataAccess()
    {
        ConnectionString = config.GetSection("ConnectionStrings")["DefaultConnection"];
    }

    internal void CreateTables()
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    internal void CreateNewStack()
    {
        Stack stack = new();

        stack.Name = AnsiConsole.Ask<string>("Enter the name of the stack: ");

        using (var conn = new SqlConnection(ConnectionString))
        {
            string insertQuery = @"INSERT INTO Stacks (Name) VALUES (@Name)";
            conn.Execute(insertQuery, new { stack.Name });
            AnsiConsole.WriteLine("Successfully created");
        }

        AnsiConsole.MarkupLine("Press any key to continue!");
        Console.ReadKey();
    }

    internal void DeleteStack()
    {
        string stackName = AnsiConsole.Ask<string>(
            "Enter the name of the stack that you want to delete: "
        );
        using (var conn = new SqlConnection(ConnectionString))
        {
            string deleteQuery = @"DELETE FROM Stacks WHERE Name = @Name";
            conn.Execute(deleteQuery, new { Name = stackName });

            string checkIfEmptyQuery = "SELECT COUNT(*) FROM Stacks";

            int rowCount = conn.ExecuteScalar<int>(checkIfEmptyQuery);

            if (rowCount == 0)
            {
                conn.Execute("DBCC CHECKIDENT('Stacks', RESEED, 0)");
            }

            AnsiConsole.WriteLine("Item has been deleted.");
            ListAllStacks();
        }

        AnsiConsole.MarkupLine("Press any key to continue!");
        Console.ReadKey();
    }

    internal void ListAllStacks()
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string selectQuery = "SELECT * FROM Stacks";

                var records = conn.Query<Stack>(selectQuery).ToList();

                foreach(var item in records)
                {
                    Console.WriteLine($"{item.Name}");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"{ex.Message}");
        }
    }
}
