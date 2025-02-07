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
                    MenuStack();
                    break;
                case Menu.FlashcardMenu:
                    MenuFlashcard();
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
        
        DataAccess data = new DataAccess();

        bool isRunning = true;
        while (isRunning)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(
                        StacksMenu.ManageAllStacks,
                        StacksMenu.CreateNewStack,
                        StacksMenu.DeleteStack,
                        StacksMenu.ReturnToMenu
                    )
            );

            switch (choice)
            {
                case StacksMenu.ManageAllStacks:
                    data.ListAllStacks();
                    break;
                case StacksMenu.CreateNewStack:
                    data.CreateNewStack();
                    break;
                case StacksMenu.DeleteStack:
                    data.DeleteStack();
                    break;
                case StacksMenu.ReturnToMenu:
                    isRunning = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    // Add flashcard menu
    internal static void MenuFlashcard()
    {
        Console.Clear();
        bool isRunning = true;
        DataAccess data = new DataAccess();

        while (isRunning)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(
                        FlashcardMenu.CreateNewFlashcard,
                        FlashcardMenu.UpdateFlashcard,
                        FlashcardMenu.DeleteFlashcard,
                        FlashcardMenu.ListAllFlashcards,
                        FlashcardMenu.ReturnToMenu
                    )
            );

            switch(choice)
            {
                case FlashcardMenu.CreateNewFlashcard:
                    data.CreateAFlashcard();
                    break;
                case FlashcardMenu.UpdateFlashcard:
                    throw new NotImplementedException();
                case FlashcardMenu.DeleteFlashcard:
                    throw new NotImplementedException();
                case FlashcardMenu.ListAllFlashcards:
                    data.ListAllFlashcards();
                    // data.GetFlashcardsWithStackNames();
                    break;
                case FlashcardMenu.ReturnToMenu:
                    isRunning = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
