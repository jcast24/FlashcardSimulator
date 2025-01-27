using Spectre.Console;

namespace FlashcardSimulator;

public class UserInterface()
{
    internal void MainMenu()
    {
        bool isRunning = true;
        while (isRunning)
        {
            Console.Clear();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Pick a choice!")
                    .AddChoices(Menu.StacksMenu, Menu.FlashcardMenu, Menu.Quit)
            );

            switch (choice)
            {
                case Menu.StacksMenu:
                    Console.Write("This goes to the stacks menu");
                    break;
                case Menu.FlashcardMenu:
                    Console.Write("List all stacks method goes here");
                    break;
                case Menu.Quit:
                    Console.Write("Goodbye!");
                    isRunning = false;
                    break;
                default:
                    Console.Write("Please choose an option!");
                    break;
            }
        }
    }

    internal static void MenuStack()
    {
        Console.Clear();

        bool isRunning = true;
        while (isRunning)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Stacks menu!")
                    .AddChoices(
                        StacksMenu.ManageAllStacks,
                        StacksMenu.CreateNewStack,
                        StacksMenu.DeleteStack
                    )
            );

            switch(choice)
            {
                case StacksMenu.ManageAllStacks:
                    Console.Write("List all the stacks here");
                    break;
                case StacksMenu.CreateNewStack:
                    Console.Write("Add method to create a new stack, once a new stack is made ask to create a new flashcard");
                    break;
                case StacksMenu.DeleteStack:
                    Console.Write("Delete stack by name");
                    break;
                default:
                    Console.Write("Please choose the correct option!");
                    break;
            }
        }
    }
}
