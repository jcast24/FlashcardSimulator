using Dapper;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace FlashcardSimulator;

class FlashcardService
{
    private readonly DataAccess _dataAccess;

    public FlashcardService(DataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    // has to return a string
    internal string GetFlashcardsWithStackNames()
    {
        try
        {
            using (var conn = new SqlConnection(_dataAccess.GetConnection()))
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

    // Change this, choose the stack by stack name, not by the Id of the stack
    // Do this because we want to show the stack name when we show all the flashcards
    private int ChooseStackById()
    {
        // List<Stack> stacks = _dataAccess.GetAllStacks();
        // var stacksArray = stacks.Select(x => x.Name).ToArray();
        //
        // var option = AnsiConsole.Prompt(
        //     new SelectionPrompt<string>().Title("Choose a stack: ").AddChoices(stacksArray)
        // );
        //
        // var stackId = stacks.Single(x => x.Name == option).Id;
        // return stackId;
        
        List<Stack> stacks = _dataAccess.GetAllStacks();

        Dictionary<string, int> stackDict = stacks.ToDictionary(s => s.Name, s => s.Id);

        string option = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Choose stack: ").AddChoices(stackDict.Keys)
                );
        
        // option: (string equivalent)
        // "Cooking" : 1
        // "Pottery" : 2
        // "Another option goes here" : 3

        int stackId = stackDict[option]; // this is basically stackDict["Cooking"] = 1 <- The id etc etc

        return stackId;
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
        InsertFlashcardIntoStack(flashcard);
    }

    // Instead of stackId, should be better to use the name of the stack.
    internal void InsertFlashcardIntoStack(Flashcard flashcard)
    {
        DataAccess db = new DataAccess();
        try
        {
            using (var conn = new SqlConnection(_dataAccess.GetConnection()))
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
    // UPDATED VERSION CHECK THIS AGAIN (2/13/2025)
    internal void DeleteAFlashcard()
    {
        ListAllFlashcards();
        try
        {
            using (var conn = new SqlConnection(_dataAccess.GetConnection()))
            {
                int getId = AnsiConsole.Ask<int>("Enter the ID of the item to delete: ");

                int itemExists = conn.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM Flashcards WHERE Id = @Id",
                    new { Id = getId }
                );

                if (itemExists == 0)
                {
                    AnsiConsole.MarkupLine("[red] Item does not exist! [/]");
                }
                else
                {
                    int rowsAffected = conn.Execute(
                        "DELETE From Flashcards WHERE Id = @Id",
                        new { Id = getId }
                    );
                    AnsiConsole.MarkupLine("[green]Item has been successfully deleted![/]");
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
        }
    }

    internal void ListAllFlashcards()
    {
        Console.Clear();
        try
        {
            using (var conn = new SqlConnection(_dataAccess.GetConnection()))
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

    internal void UpdateFlashcard()
    {
        Console.Clear();
        ListAllFlashcards();

        using (var conn = new SqlConnection(_dataAccess.GetConnection()))
        {
            bool doesItemExist;
            int getId;
            do
            {
                getId = AnsiConsole.Ask<int>("Enter the ID of the flashcard to update: ");

                doesItemExist =
                    conn.ExecuteScalar<int>(
                        "SELECT COUNT(1) FROM Flashcards WHERE Id = @Id",
                        new { Id = getId }
                    ) > 0;

                if (!doesItemExist)
                {
                    AnsiConsole.MarkupLine("[red]Item does not exist! Please enter a valid ID.[/]");
                }
            } while (!doesItemExist);

            string updatedQuestion = AnsiConsole.Ask<string>("Re-enter the question: ");
            string updatedAnswer = AnsiConsole.Ask<string>("Re-enter the answer: ");

            string updatedQuery =
                "UPDATE Flashcards SET Question=@Question, Answer=@Answer WHERE Id = @Id";

            int rowsAffected = conn.Execute(
                updatedQuery,
                new
                {
                    Question = updatedQuestion,
                    Answer = updatedAnswer,
                    Id = getId,
                }
            );
            if (rowsAffected > 0)
            {
                AnsiConsole.MarkupLine("[green]Item has been successfully updated[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Update failed. Please try again![/]");
            }
        }
    }
}
