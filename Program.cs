namespace FlashcardSimulator;

class Program
{
    static void Main(string[] args)
    {
        DataAccess db = new DataAccess();
        db.CreateTables();
    }
}
