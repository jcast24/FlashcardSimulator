using Spectre.Console;

namespace FlashcardSimulator;

public class UserInterface()
{
    internal void MainMenu()
    {
        string[] menuChoices = new string[3]
        {
            "Create a new stack",
            "List all stacks",
            "Delete a stack",
        };

        while (true)
        {
            Console.Clear();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Pick a choice!").AddChoices(menuChoices)
            );

            switch(choice)
            {
                case "Create a new stack":
                    Console.Write("Create new stack method goes here");
                    break;
                case "List all stacks":
                    Console.Write("List all stacks method goes here");
                    break;
                case "Delete a stack":
                    Console.Write("Delete a stack");
                    break;
                default:
                    Console.Write("Please choose an option!");
                    break;
            }
        }
    }
}
