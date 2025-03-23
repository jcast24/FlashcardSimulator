namespace FlashcardSimulator.Models.DTO;

public class FlashcardDto
{
    public int FlashcardId { get; set; }
    public string Question { get; set; } = String.Empty;
    public string Answer { get; set; } = String.Empty;
    public string StackName { get; set; } = String.Empty;
}