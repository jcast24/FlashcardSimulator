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

    internal List<Stack> ListAllStacks()
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string selectQuery = "SELECT * FROM Stacks ORDER BY Id";

                var records = conn.Query<Stack>(selectQuery).ToList();

                foreach (var item in records)
                {
                    Console.WriteLine($"{item.Id} - {item.Name}");
                }
                return records;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return new List<Stack>();
        }
    }

    // Change this, choose the stack by stack name, not by the Id of the stack
    private static int ChooseStack()
    {
        var dataAccess = new DataAccess();

        var stacks = dataAccess.ListAllStacks();
        var stacksArray = stacks.Select(x => x.Name).ToArray();

        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>().Title("Choose a stack: ").AddChoices(stacksArray)
        );
        var stackId = stacks.Single(x => x.Name == option).Id;
        return stackId;
    }

    internal void CreateAFlashcard()
    {
        // List all the stacks
        // choose which stack based off of ID
        Flashcard flashcard = new();
        
        // Connect the stackID from Stack model to Flashcard model
        flashcard.StackId = ChooseStack();
        flashcard.Question = AnsiConsole.Ask<string>("Enter your question: ");
        flashcard.Answer = AnsiConsole.Ask<string>("Enter your answer: ");

        DataAccess data = new DataAccess();
        data.InsertFlashcardIntoStack(flashcard);
    }

    internal void InsertFlashcardIntoStack(Flashcard flashcard)
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string insertFlashcardQuery =
                    @"INSERT INTO Flashcards (Question, Answer, StackId) VALUES (@Question, @Answer, @StackId)";
                conn.Execute(
                    insertFlashcardQuery,
                    new
                    {
                        flashcard.Question,
                        flashcard.Answer,
                        flashcard.StackId,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
    }

    internal void ListAllFlashcards()
    {
        try {
            using(var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string listFlashcardsQuery = "SELECT * FROM Flashcards";
                
                var records = conn.Query<Flashcard>(listFlashcardsQuery).ToList();

                foreach (var item in records)
                {
                    Console.WriteLine($"{item.StackId} - {item.Id} - {item.Question} - {item.Answer}");
                }
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
    }
}
