using FlashcardSimulator.Models;
using FlashcardSimulator.Views;

namespace FlashcardSimulator;

class Program
{
    static void Main(string[] args)
    {
        var db = new DataAccess();
        var ui = new UserInterface();
        db.CreateTables();
        ui.MainMenu();
    }
}