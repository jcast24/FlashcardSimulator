using Dapper;
using FlashcardSimulator.Models;
using FlashcardSimulator.Models.DTO;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace FlashcardSimulator.Services;

public class FlashcardService
{
    private readonly DataAccess _dataAccess;
    private readonly StackService _stackService;

    public FlashcardService(DataAccess dataAccess, StackService stackService)
    {
        _dataAccess = dataAccess;
        _stackService = stackService;
    }

    // Get All Flashcards
    internal List<FlashcardDto> GetAllFlashcards()
    {
        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());
            connection.Open();
            string selectQuery = "SELECT * FROM FlashcardView ORDER BY FlashcardId";
            var records = connection.Query<FlashcardDto>(selectQuery).ToList();

            return records;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return new List<FlashcardDto>();
        }
    }

    // Show All Flashcards (this is for the menu)
    internal void ShowAllFlashcards()
    {
        var getAllFlashcards = GetAllFlashcards();

        if (!getAllFlashcards.Any())
        {
            AnsiConsole.MarkupLine("[red]There are no flashcards available[/]");
        }

        foreach (var flashcard in getAllFlashcards)
        {
            AnsiConsole.MarkupLine(
                $"Flashcard ID: {flashcard.FlashcardId}\nQuestion: {flashcard.Question}\nAnswer: {flashcard.Answer}");
        }
    }

    // Get Flashcard by ID
    internal int GetFlashcardById()
    {
        var flashcards = GetAllFlashcards();
        var flashcardsArray = flashcards.Select(x => x.Question).ToArray();

        var option = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Choose a flashcard: ")
            .AddChoices(flashcardsArray));

        var flashcardId = flashcards.Single(x => x.Question == option).FlashcardId;
        return flashcardId;
    }


    // Create a new Flashcard
    internal void CreateAFlashcard()
    {
        Flashcard newFlashcard = new();
        newFlashcard.StackId = _stackService.ChooseStackById();

        newFlashcard.Question = AnsiConsole.Ask<string>("What is your question? ");

        while (string.IsNullOrEmpty(newFlashcard.Question))
        {
            newFlashcard.Question = AnsiConsole.Ask<string>("Question can't be empty. Try again: ");
        }


        newFlashcard.Answer = AnsiConsole.Ask<string>("What is your answer? ");

        while (string.IsNullOrEmpty(newFlashcard.Answer))
        {
            newFlashcard.Answer = AnsiConsole.Ask<string>("Question can't be empty. Try again: ");
        }

        InsertFlashcardIntoStack(newFlashcard);
    }

    // Insert into Database
    internal void InsertFlashcardIntoStack(Flashcard flashcard)
    {
        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());
            connection.Open();
            string insertQuery =
                "INSERT INTO Flashcards (Question, Answer, StackId) VALUES (@Question, @Answer, @StackId)";
            connection.Execute(insertQuery, new { flashcard.Question, flashcard.Answer, flashcard.StackId });
            AnsiConsole.Markup($"[green]Successfully added flashcard into stack[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Error InsertFlashcardIntoStack: {e.Message}[/]");
        }

        AnsiConsole.MarkupLine("\n[cyan]Press any key to continue[/]");
        Console.ReadKey();
    }

    // Update a flashcard based on id
    internal void UpdateFlashcard()
    {
        Console.Clear();

        using var connection = new SqlConnection(_dataAccess.GetConnection());

        int getId = GetFlashcardById();

        var updatedQuestion = AnsiConsole.Ask<string>("Re-enter question: ");
        var updatedAnswer = AnsiConsole.Ask<string>("Re-enter answer: ");

        string updateQuery = "UPDATE Flashcards SET Question=@Question, Answer=@Answer WHERE Id=@Id";
        connection.Execute(updateQuery, new { Question = updatedQuestion, Answer = updatedAnswer, Id = getId });
        AnsiConsole.MarkupLine("[green]Successfully updated item[/]");
    }

    // Delete a flashcard based on id 
    internal void DeleteFlashcard()
    {
        Console.Clear();

        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());

            // basically we need to create a checker to check if there are any items in the database
            // if there aren't return a message
            var dbCount = "SELECT COUNT(*) FROM Flashcards";
            var count = connection.ExecuteScalar<int>(dbCount);

            if (count == 0)
            {
                // Might need to change Flashcards to FlashcardsDTO
                var reseedQuery = "DBCC CHECKIDENT('Flashcards', RESEED, 0);";
                connection.Execute(reseedQuery);
                AnsiConsole.MarkupLine("[red]There are no items to delete[/]");
            }
            else
            {
                int getId = GetFlashcardById();

                var reseedQuery = "DBCC CHECKIDENT('Flashcards', RESEED, 0);";
                connection.Execute(reseedQuery);

                var deleteQuery = "DELETE FROM Flashcards WHERE Id=@Id";

                connection.Execute(deleteQuery, new { Id = getId });
                
                AnsiConsole.MarkupLine("[green]Successfully deleted item[/]");
            }

            // if items in database = 0 or items in database.Count == 0
            // run this sql RESEED command
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Error DeleteFlashcard: {e.Message}[/]");
        }
    }
}