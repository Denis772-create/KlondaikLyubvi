namespace KlondaikLyubvi.Models;

// DTO классы для API
public class ExchangeRequestDto
{
    public int RequesterId { get; set; }
    public int RequestedServiceId { get; set; }
    public int? OfferedServiceId { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Message { get; set; }
}

public class ExchangeResponseDto
{
    public int ExchangeId { get; set; }
    public bool Accept { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime? NewScheduledDate { get; set; }
}

public class ExchangeCompleteDto
{
    public int ExchangeId { get; set; }
    public int? Rating { get; set; }
    public string? Review { get; set; }
}

public class ServiceOfferCreateDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Emoji { get; set; } = "💝";
    public string Category { get; set; } = "Общее";
}

public class ReactionRequest
{
    public string Emoji { get; set; } = "";
    public int UserId { get; set; }
}

public class TagRequest
{
    public string Tag { get; set; } = "";
}

public class CaptionRequest
{
    public string Caption { get; set; } = "";
}

public class CalendarEventRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public string Emoji { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Description { get; set; }
}