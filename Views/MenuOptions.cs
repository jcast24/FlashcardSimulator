namespace FlashcardSimulator.Views;

internal struct Menu
{
    public const string StacksMenu = "Manage Stacks";
    public const string FlashcardMenu = "Manage Flashcard Menu";
    public const string StudySession = "Study Session";
    public const string Quit = "Quit";
}

internal struct StacksMenu
{
    public const string CreateNewStack = "Create new stack";
    public const string ManageAllStacks = "List all Stacks";
    public const string DeleteStack = "Delete Stack";
    public const string ReturnToMenu = "Return to Main Menu";
}

internal struct FlashcardMenu
{
    public const string CreateNewFlashcard = "Create new flashcard";
    public const string UpdateFlashcard = "Update existing flashcard";
    public const string DeleteFlashcard = "Delete Flashcard";
    public const string ListAllFlashcards = "List all Flashcards";
    public const string ReturnToMenu = "Return to Main Menu";
}

internal struct StudySessionMenu
{
    public const string CreateStudySession = "Create a new study session";
    public const string ShowAllStudySessions = "Show all the study sessions";
    public const string DeleteStudySession = "Delete a study session";
    public const string ReturnToMenu = "Return to Main Menu";
}