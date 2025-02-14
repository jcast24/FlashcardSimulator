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

                if (records.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No stacks exists, please create one! [/]");
                }

                foreach (var item in records)
                {
                    Console.WriteLine($"{item.Name}");
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
    // Do this because we want to show the stack name when we show all the flashcards
    private static int ChooseStackById()
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

    // has to return a string
    internal string GetFlashcardsWithStackNames()
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                string sql =
                    "SELECT FlashcardId as Id, Question, Answer, StackName FROM FlashcardView;";
                var records = conn.Query<FlashcardDTO>(sql).ToList();

                foreach (var rec in records)
                {
                    return $"{rec.StackName}";
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.Write($"[red]{ex.Message}[/]");
        }
        return "Not found";
    }

    internal void CreateAFlashcard()
    {
        // List all the stacks
        // choose which stack based off of ID
        Flashcard flashcard = new();

        // Connect the stackID from Stack model to Flashcard model
        flashcard.StackId = ChooseStackById();

        flashcard.Question = AnsiConsole.Ask<string>("Enter your question: ");
        flashcard.Answer = AnsiConsole.Ask<string>("Enter your answer: ");

        DataAccess data = new DataAccess();
        data.InsertFlashcardIntoStack(flashcard);
    }

    // Instead of stackId, should be better to use the name of the stack.
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

    // Add input validation when deleting
    // if I do delete flashcard 5 and 5 isn't there then it should give me 
    // "flashcard 5 does not exist"
    internal void DeleteAFlashcard()
    {
        try {
            using (var conn = new SqlConnection(ConnectionString))
            {
                ListAllFlashcards();

                conn.Open();

                int getId = AnsiConsole.Ask<int>("Enter the Id of the flashcard you would like to delete: ");
        
                string deleteQuery = "DELETE FROM Flashcards WHERE Id=@Id";

                string checkIfEmptyQuery = "SELECT COUNT(*) FROM Flashcards";

                int rowCount = conn.ExecuteScalar<int>(checkIfEmptyQuery);

                if (rowCount == 0)
                {
                    conn.Execute("DBCC CHECKIDENT('Flashcards', RESEED, 0)");
                }

                conn.Execute(deleteQuery, new {Id = getId});

                AnsiConsole.MarkupLine($"[green]Item Id: {getId} has been successfully deleted![/]");

                ListAllFlashcards();
            }
        } catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
        }
    }

    internal void ListAllFlashcards()
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string listFlashcardsQuery = "SELECT * FROM FlashcardView";

                var records = conn.Query<FlashcardDTO>(listFlashcardsQuery).ToList();

                if (records.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No flashcards currently exist.[/]");
                }

                foreach (var item in records)
                {
                    Console.WriteLine(
                        $"{item.StackName} - {item.FlashcardId} - {item.Question} - {item.Answer}"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
    }
}
