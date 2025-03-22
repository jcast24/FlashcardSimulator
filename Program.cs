using FlashcardSimulator.Views;

namespace FlashcardSimulator;

class Program
{
    static void Main(string[] args)
    {
        DataAccess db = new DataAccess();
        UserInterface ui = new UserInterface();
        db.CreateTables();
        ui.MainMenu();
    }
}
