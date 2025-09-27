using KlondaikLyubvi.Data;
using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Services;

public class RomanceExchangeService(AppDbContext db, TelegramService telegram)
{
    private readonly AppDbContext _db = db;
    private readonly TelegramService _telegram = telegram;

    // Убираем кредиты - теперь прямой обмен услугами

    public async Task<List<ServiceOffer>> GetAvailableServicesAsync(int userId)
    {
        return await _db.ServiceOffers
            .Include(s => s.User)
            .Where(s => s.UserId != userId && s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<ServiceOffer>> GetMyServicesAsync(int userId)
    {
        return await _db.ServiceOffers
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> RequestServiceAsync(int requesterId, int requestedServiceId, int? offeredServiceId = null, DateTime? scheduledDate = null, string? message = null)
    {
        var requester = await _db.Users.FindAsync(requesterId);
        var requestedService = await _db.ServiceOffers.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == requestedServiceId);
        
        if (requester == null || requestedService == null || !requestedService.IsActive) 
            return false;

        ServiceOffer? offeredService = null;
        if (offeredServiceId.HasValue)
        {
            offeredService = await _db.ServiceOffers.FirstOrDefaultAsync(s => s.Id == offeredServiceId.Value && s.UserId == requesterId);
        }

        // Создаем запрос на обмен
        var exchange = new ServiceExchange
        {
            RequesterId = requesterId,
            ProviderId = requestedService.UserId,
            RequestedServiceId = requestedServiceId,
            OfferedServiceId = offeredServiceId,
            RequestDate = DateTime.UtcNow,
            ScheduledDate = scheduledDate,
            Status = ExchangeStatus.Pending,
            RequestMessage = message
        };

        _db.ServiceExchanges.Add(exchange);
        await _db.SaveChangesAsync();

        // Уведомления в Telegram
        await _telegram.SendExchangeRequestAsync(
            requestedService.UserId, 
            requesterId, 
            requestedService.Name, 
            requestedService.Emoji,
            offeredService?.Name,
            offeredService?.Emoji);

        await _telegram.SendExchangeRequestConfirmationAsync(
            requesterId, 
            requestedService.UserId, 
            requestedService.Name, 
            requestedService.Emoji,
            offeredService?.Name,
            offeredService?.Emoji);

        return true;
    }

    public async Task<bool> RespondToRequestAsync(int exchangeId, bool accept, string? responseMessage = null, DateTime? newScheduledDate = null)
    {
        var exchange = await _db.ServiceExchanges
            .Include(e => e.Requester)
            .Include(e => e.RequestedService)
            .Include(e => e.OfferedService)
            .FirstOrDefaultAsync(e => e.Id == exchangeId);

        if (exchange == null || exchange.Status != ExchangeStatus.Pending)
            return false;

        if (accept)
        {
            exchange.Status = ExchangeStatus.Accepted;
            exchange.ResponseMessage = responseMessage;
            if (newScheduledDate.HasValue)
                exchange.ScheduledDate = newScheduledDate;

            await _telegram.SendExchangeAcceptedAsync(
                exchange.RequesterId,
                exchange.ProviderId,
                exchange.RequestedService?.Name ?? "",
                exchange.RequestedService?.Emoji ?? "",
                exchange.OfferedService?.Name,
                exchange.OfferedService?.Emoji);
        }
        else
        {
            exchange.Status = ExchangeStatus.Declined;
            exchange.ResponseMessage = responseMessage;

            await _telegram.SendExchangeRejectedAsync(
                exchange.RequesterId,
                exchange.RequestedService?.Name ?? "",
                responseMessage);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteExchangeAsync(int exchangeId, int? rating = null, string? review = null)
    {
        var exchange = await _db.ServiceExchanges
            .Include(e => e.Requester)
            .Include(e => e.Provider)
            .Include(e => e.RequestedService)
            .Include(e => e.OfferedService)
            .FirstOrDefaultAsync(e => e.Id == exchangeId);

        if (exchange == null || exchange.Status != ExchangeStatus.Accepted)
            return false;

        exchange.Status = ExchangeStatus.Completed;
        exchange.CompletedDate = DateTime.UtcNow;
        exchange.Rating = rating;
        exchange.Review = review;

        await _db.SaveChangesAsync();

        // Уведомления
        await _telegram.SendExchangeCompletedAsync(
            exchange.ProviderId,
            exchange.RequesterId,
            exchange.RequestedService?.Name ?? "",
            exchange.OfferedService?.Name);

        return true;
    }

    public async Task<List<ServiceExchange>> GetMyExchangesAsync(int userId)
    {
        return await _db.ServiceExchanges
            .Include(e => e.Requester)
            .Include(e => e.Provider)
            .Include(e => e.RequestedService)
            .Include(e => e.OfferedService)
            .Where(e => e.RequesterId == userId || e.ProviderId == userId)
            .OrderByDescending(e => e.RequestDate)
            .ToListAsync();
    }

    public async Task<List<ServiceExchange>> GetPendingRequestsAsync(int userId)
    {
        return await _db.ServiceExchanges
            .Include(e => e.Requester)
            .Include(e => e.RequestedService)
            .Include(e => e.OfferedService)
            .Where(e => e.ProviderId == userId && e.Status == ExchangeStatus.Pending)
            .OrderBy(e => e.RequestDate)
            .ToListAsync();
    }

    public async Task<List<ServiceExchange>> GetActiveExchangesAsync(int userId)
    {
        return await _db.ServiceExchanges
            .Include(e => e.Requester)
            .Include(e => e.Provider)
            .Include(e => e.RequestedService)
            .Include(e => e.OfferedService)
            .Where(e => (e.RequesterId == userId || e.ProviderId == userId) && e.Status == ExchangeStatus.Accepted)
            .OrderBy(e => e.ScheduledDate ?? e.RequestDate)
            .ToListAsync();
    }

    // Добавляем методы для управления услугами
    public async Task<ServiceOffer> CreateServiceOfferAsync(int userId, string name, string description, string emoji, string category)
    {
        var serviceOffer = new ServiceOffer
        {
            Name = name,
            Description = description,
            Emoji = emoji,
            Category = category,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ServiceOffers.Add(serviceOffer);
        await _db.SaveChangesAsync();
        return serviceOffer;
    }

    public async Task<bool> UpdateServiceOfferAsync(int serviceId, int userId, string name, string description, string emoji, string category)
    {
        var service = await _db.ServiceOffers.FirstOrDefaultAsync(s => s.Id == serviceId && s.UserId == userId);
        if (service == null) return false;

        service.Name = name;
        service.Description = description;
        service.Emoji = emoji;
        service.Category = category;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteServiceOfferAsync(int serviceId, int userId)
    {
        var service = await _db.ServiceOffers.FirstOrDefaultAsync(s => s.Id == serviceId && s.UserId == userId);
        if (service == null) return false;

        _db.ServiceOffers.Remove(service);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleServiceActiveAsync(int serviceId, int userId)
    {
        var service = await _db.ServiceOffers.FirstOrDefaultAsync(s => s.Id == serviceId && s.UserId == userId);
        if (service == null) return false;

        service.IsActive = !service.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}