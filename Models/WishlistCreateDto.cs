namespace KlondaikLyubvi.Models;

public class WishlistCreateDto
{
    public int UserId { get; set; }
    public string Occasion { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Note { get; set; }
}


