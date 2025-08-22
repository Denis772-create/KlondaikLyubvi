namespace KlondaikLyubvi.Models;

public class StoreItemCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Price { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public int UserId { get; set; }
}