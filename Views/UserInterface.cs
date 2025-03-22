using FlashcardSimulator.Services;
using Spectre.Console;

namespace FlashcardSimulator.Views;

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
        StackService stackService = new StackService(data);
        
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
                    stackService.ShowAllStacks();
                    break;
                case StacksMenu.CreateNewStack:
                    stackService.CreateNewStack();
                    break;
                case StacksMenu.DeleteStack:
                    stackService.DeleteStack();
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
        DataAccess db = new DataAccess();
        StackService stack = new StackService(db);
        FlashcardService flashService = new FlashcardService(db, stack);

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
                    flashService.CreateAFlashcard();
                    break;
                case FlashcardMenu.UpdateFlashcard:
                    flashService.UpdateFlashcard();
                    break;
                case FlashcardMenu.DeleteFlashcard:
                    // flashService.DeleteAFlashcard();
                    break;
                case FlashcardMenu.ListAllFlashcards:
                    flashService.ShowAllFlashcards();
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
