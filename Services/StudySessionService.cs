using System.Globalization;
using Dapper;
using FlashcardSimulator.Models;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace FlashcardSimulator.Services;

public class StudySessionService
{
    private readonly DataAccess _dataAccess;
    private readonly StackService _stackService;
    private readonly FlashcardService _flashcardService;
    
    // Create a new study session
    public StudySessionService(
        StackService stackService,
        FlashcardService flashcardService,
        DataAccess dataAccess
    )
    {
        _stackService = stackService;
        _flashcardService = flashcardService;
        _dataAccess = dataAccess;
    }

    internal void CreateNewStudySession()
    {
        var stackId = _stackService.ChooseStackById();
        // var flashcardId = _flashcardService.GetFlashcardById();
        var flashcards = _flashcardService.GetAllFlashcards();

        var correctAnswers = 0;

        var studySession = new StudySessions();
        studySession.StackId = stackId;
        // studySession.FlashcardId = flashcardId;
        studySession.Date = DateTime.Now;

        studySession.AmountOfQuestions = flashcards.Count;

        foreach (var flashcard in flashcards)
        {
            var answer = AnsiConsole.Ask<string>($"{flashcard.Question}");
            while (string.IsNullOrEmpty(answer))
            {
                answer = AnsiConsole.Ask<string>($"Answer can't be empty. {flashcard.Question}");
            }

            if (string.Equals(answer.Trim(), flashcard.Answer, StringComparison.OrdinalIgnoreCase))
            {
                correctAnswers++;
                AnsiConsole.MarkupLine("[green]Correct![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]incorrect![/]");
            }

            studySession.CorrectAnswersAmount = correctAnswers;
            Console.WriteLine($"You've got {correctAnswers} out of {flashcards.Count}");
            studySession.Time = DateTime.Now - studySession.Date;
        }

        InsertStudySessionIntoDb(studySession);
        Console.WriteLine("Successfully created!");
    }

    // Insert into Database
    internal void InsertStudySessionIntoDb(StudySessions studySession)
    {
        using var connection = new SqlConnection(_dataAccess.GetConnection());
        connection.Open();
        string insertSessionQuery =
            @"
            INSERT INTO StudySessions (StackId, Date, AmountOfQuestions, CorrectAnswersAmount, Time)
            VALUES (@StackId, @Date, @AmountOfQuestions, @CorrectAnswersAmount, @Time);
            ";

        connection.Execute(
            insertSessionQuery,
            new
            {
                studySession.StackId,
                studySession.Date,
                studySession.AmountOfQuestions,
                studySession.CorrectAnswersAmount,
                studySession.Time,
            }
        );

        AnsiConsole.MarkupLine("[green]Successfully added Session into DB[/]");
    }

    // Get Study sessions from Database
    private IEnumerable<StudySessions> GetAllStudySessions()
    {
        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());
            string selectQuery = "SELECT * FROM StudySessions";
            var records = connection.Query<StudySessions>(selectQuery);
            return records;
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Error GetAllStudySessions: {e.Message} [/]");
            return new List<StudySessions>();
        }
    }

    // Show all the Study Sessions (basically output to console)
    internal void ShowAllStudySessions()
    {
        string[] colNames = ["Session ID", "Stack ID", "Questions", "Correct", "Percent", "Time", "Date"];

        var studySessions = GetAllStudySessions();

        var table = new Table();
        
        table.AddColumns(colNames);
        foreach (var session in studySessions)
        {
            table.AddRow(
                session.Id.ToString(),
                session.StackId.ToString(),
                session.AmountOfQuestions.ToString(),
                session.CorrectAnswersAmount.ToString(),
                session.Percentage.ToString(),
                session.Time.ToString(),
                session.Date.ToString(CultureInfo.InvariantCulture)
                );
        }

        table.Border(TableBorder.Rounded);
        table.Title("[underline green]Study Sessions![/]");
        table.Centered();
        AnsiConsole.Write(table);
        
        AnsiConsole.WriteLine("Press any key to continue");
        Console.ReadKey();
    }

    // delete study session
    internal void DeleteStudySession()
    {
        using var connection = new SqlConnection(_dataAccess.GetConnection());
        var deleteId = AnsiConsole.Ask<int>("Which session would you like to delete? ");
        var deleteQuery = "DELETE FROM StudySessions WHERE Id=@Id";
        connection.Execute(deleteQuery, new { Id = deleteId });
        AnsiConsole.MarkupLine("[green] Successfully deleted item! [/]");
    }
}