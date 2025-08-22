using KlondaikLyubvi.Data;
using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Services;

public class LoveStoreService(AppDbContext db, TelegramService telegram)
{
    private readonly AppDbContext _db = db;
    private readonly TelegramService _telegram = telegram;

    public async Task<int> GetBalanceAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user?.LovePoints ?? 0;
    }

    public async Task<bool> BuyAsync(int userId, int storeItemId, bool isGift = false, int? toUserId = null, DateTime? executionDate = null, DateTime? giftStartDate = null, DateTime? giftEndDate = null, int giftCount = 1)
    {
        var user = await _db.Users.FindAsync(userId);
        var item = await _db.StoreItems.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == storeItemId);
        if (user == null || item == null) return false;
        int totalPrice = item.Price * (isGift ? giftCount : 1);
        if (user.LovePoints < totalPrice) return false;
        user.LovePoints -= totalPrice;
        User? recipient = null;
        if (isGift && toUserId.HasValue && toUserId != userId)
        {
            recipient = await _db.Users.FindAsync(toUserId.Value);
            if (recipient != null)
            {
                recipient.LovePoints += item.Price * giftCount;
            }
        }
        _db.LoveCoinTransactions.Add(new LoveCoinTransaction
        {
            UserId = userId,
            StoreItemId = storeItemId,
            Date = DateTime.UtcNow,
            IsGift = isGift,
            ToUserId = toUserId,
            ExecutionDate = executionDate,
            IsExecuted = false,
            GiftStartDate = giftStartDate,
            GiftEndDate = giftEndDate,
            GiftCount = giftCount
        });
        await _db.SaveChangesAsync();
        // Telegram notifications
        if (isGift && toUserId.HasValue && toUserId != userId)
        {
            // Получатель подарка
            await _telegram.SendMessageAsync(
                toUserId.Value,
                $"🎁 Тебе прилетел подарочек: {item.Emoji} «{item.Name}» x{giftCount}!\nОт: {user?.DisplayName ?? "кто-то особенный"} 💞\nЗагляни в магазин любви — там тебя ждёт немного магии ✨");
            // Даритель
            await _telegram.SendMessageAsync(
                userId,
                $"🎁 Ты подарил(а) {item.Emoji} «{item.Name}» x{giftCount} для {recipient?.DisplayName ?? "любимого человека"} 💖\nПусть это сделает день ещё теплее ✨");
        }
        else
        {
            // Покупатель
            await _telegram.SendMessageAsync(
                userId,
                $"🛒 Готово! {item.Emoji} «{item.Name}» теперь твоя(твой) 💞\nКогда захочешь — исполни или обменяй в магазине любви.");
            // Владелец товара (если не сам себе)
            if (item.UserId != userId)
            {
                await _telegram.SendMessageAsync(
                    item.UserId,
                    $"🛒 У тебя только что купили {item.Emoji} «{item.Name}» 💕\nПокупатель: {user?.DisplayName ?? "кто-то особенный"}.\nЗагляни в магазин — там тебя ждёт приятность!");
            }
        }
        return true;
    }

    public async Task<List<LoveCoinTransaction>> GetHistoryAsync(int userId)
    {
        return await _db.LoveCoinTransactions
            .Include(t => t.StoreItem)
            .Include(t => t.ToUser)
            .Where(t => t.UserId == userId || t.ToUserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }
}