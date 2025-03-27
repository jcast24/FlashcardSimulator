namespace FlashcardSimulator.Models;

public class StudySessions
{
    public int FlashcardId { get; set; }
    public int StackId { get; set; }
    public DateTime Date { get; set; }
    public int AmountOfQuestions { get; set; }
    public int CorrectAnswersAmount { get; set; }
    public int Percentage { get; set; }
    public TimeSpan Time { get; set; }
    
}