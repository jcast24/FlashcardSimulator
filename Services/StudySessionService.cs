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
    
    // Housekeeping
    // Get Study sessions from Database
    // Show all the Study Sessions (basically output to console)
    // delete study session

    // Create a new study session
    public StudySessionService(StackService stackService, FlashcardService flashcardService, DataAccess dataAccess)
    {
        _stackService = stackService;
        _flashcardService = flashcardService;
        _dataAccess = dataAccess;
    }

    internal void CreateNewStudySession()
    {
        var stackId = _stackService.ChooseStackById();
        var flashcardId = _flashcardService.GetFlashcardById();
        var flashcards = _flashcardService.GetAllFlashcards();

        var correctAnswers = 0;

        var studySession = new StudySession();
        studySession.StackId = stackId;
        studySession.FlashcardId = flashcardId;
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
    internal void InsertStudySessionIntoDb(StudySession studySession)
    {
        using var connection = new SqlConnection(_dataAccess.GetConnection());
        string insertSessionQuery = """
                                    INSERT INTO StudySessions (FlashcardId, StackId, Date, AmountOfQuestions, CorrectAnswersAmount, Time)
                                                VALUES (@FlashcardId, @StackId, @Date, @AmountOfQuestions, @CorrectAnswersAmount, @Time);
                                    """;

        connection.Execute(insertSessionQuery, new
        {
            studySession.FlashcardId,
            studySession.StackId,
            studySession.Date,
            studySession.AmountOfQuestions,
            studySession.CorrectAnswersAmount,
            studySession.Time,
        });
        
        AnsiConsole.MarkupLine("[green]Successfully added Session into DB[/]");
    }


    // Get Study sessions from Database
    // Show all the Study Sessions (basically output to console)
    // delete study session
}