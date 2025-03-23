using Dapper;
using FlashcardSimulator.Models;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace FlashcardSimulator.Services;

public class StackService
{
    private readonly DataAccess _dataAccess;

    public StackService(DataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    private IEnumerable<Stack> GetAllStacks()
    {
        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());
            connection.Open();
            string selectQuery = "SELECT * FROM Stacks ORDER BY Id";
            var records = connection.Query<Stack>(selectQuery);

            return records;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return new List<Stack>();
        }
    }

    internal void ShowAllStacks()
    {
        var stacks = GetAllStacks();
        foreach (var stack in stacks)
        {
            Console.WriteLine($"Stack ID: {stack.Id}\nName: {stack.Name}\n");
        }

        if (!stacks.Any())
        {
            Console.WriteLine("There are no stacks available");
        }
    }

    internal void CreateNewStack()
    {
        Stack newStack = new Stack();

        var name = AnsiConsole.Ask<string>("What is the name of the new stack?");

        while (string.IsNullOrEmpty(name))
        {
            name = AnsiConsole.Ask<string>("Stack name can't be empty, please try again: ");
        }

        newStack.Name = name;

        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());
            connection.Open();
            string insertQuery = @"INSERT INTO Stacks(Name) VALUES (@Name)";
            connection.Execute(insertQuery, new { newStack.Name });
            AnsiConsole.MarkupLine("[green]Successfully created[/]");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }


    internal int ChooseStackById()
    {
        var stacks = GetAllStacks();
        var stacksArray = stacks.Select(x => x.Name).ToArray();

        var option = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Choose a stack: ")
            .AddChoices(stacksArray));

        var stackId = stacks.Single(x => x.Name == option).Id;
        return stackId;
    }


    internal void DeleteStack()
    {
        var getId = ChooseStackById();

        try
        {
            using var connection = new SqlConnection(_dataAccess.GetConnection());
            string deleteQuery = "DELETE FROM Stacks WHERE Id = @Id";
            connection.Execute(deleteQuery, new { Id = getId });
            AnsiConsole.MarkupLine("[green]Successfully deleted item[/]");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error for DeleteStack: {e.Message}");
        }
    }
}