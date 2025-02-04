namespace FlashcardSimulator;

internal struct Menu
{
    public const string StacksMenu = "Manage Stacks";
    public const string FlashcardMenu = "Manage Flashcard Menu";
    // public const string StudyArea = "Study Session";
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
}
