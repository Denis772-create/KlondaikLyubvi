namespace KlondaikLyubvi.Shared;

public record BuyRequest(
    int UserId,
    int StoreItemId,
    bool IsGift,
    int? ToUserId,
    DateTime? ExecutionDate,
    DateTime? GiftStartDate,
    DateTime? GiftEndDate,
    int GiftCount
);