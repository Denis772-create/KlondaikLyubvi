namespace KlondaikLyubvi.Shared;

public class LoveNoteDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public string? UserDisplayName { get; set; }
}