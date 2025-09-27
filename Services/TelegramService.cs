using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace KlondaikLyubvi.Services;

public class TelegramService
{
    private readonly TelegramBotClient _botClient = new(BotToken);
    private const string BotToken = "8303606158:AAG3L8iV0iZvNMeKFvnBdn5Ma_vCC8K-pdI";
    
    private readonly Dictionary<int, long> _userChatIds = new()
    {
        { 1, 721196467 }, // Denis
        { 2, 260558500 }  // Liza   
    };
    
    private readonly Dictionary<int, UserInfo> _userInfo = new()
    {
        { 1, new UserInfo { Name = "Денис", Gender = "male", PartnerName = "Лиза" } },
        { 2, new UserInfo { Name = "Лиза", Gender = "female", PartnerName = "Денис" } }
    };

    public async Task SendMessageAsync(int userId, string message)
    {
        if (_userChatIds.TryGetValue(userId, out var chatId))
        {
            try
            {
                await _botClient.SendMessage(chatId, message, parseMode: ParseMode.Html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telegram] Send failed: {ex.Message}");
            }
        }
    }

    public async Task SendMessageWithUrlButtonAsync(int userId, string message, string buttonText, string url)
    {
        if (_userChatIds.TryGetValue(userId, out var chatId))
        {
            try
            {
                var markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(buttonText, url));
                await _botClient.SendMessage(chatId, message, parseMode: ParseMode.Html, replyMarkup: markup);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telegram] Send failed: {ex.Message}");
            }
        }
    }
    
    public async Task SendAnimatedMessageAsync(int userId, string message)
    {
        if (_userChatIds.TryGetValue(userId, out var chatId))
        {
            try
            {
                // Отправляем анимированное сообщение с эффектом печати
                await _botClient.SendChatAction(chatId, ChatAction.Typing);
                await Task.Delay(1500); // Имитируем печать
                await _botClient.SendMessage(chatId, message, parseMode: ParseMode.Html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telegram] Send failed: {ex.Message}");
            }
        }
    }
    
    // Персонализированные сообщения для признаний
    public async Task SendLoveNoteNotificationAsync(int recipientId, int senderId, string link)
    {
        var recipient = GetUserInfo(recipientId);
        var sender = GetUserInfo(senderId);
        
        var messages = new[]
        {
            $"💌 <b>{sender.Name}</b> написал тебе что-то особенное!\n\n✨ Открой, когда будешь готова улыбнуться 😊",
            $"💕 У тебя новое признание от <b>{sender.Name}</b>!\n\n🌟 Твоё сердце сейчас точно забьётся быстрее 💓",
            $"🎀 <b>{sender.Name}</b> поделился с тобой своими чувствами!\n\n💖 Приготовься к порции нежности ☺️"
        };
        
        var randomMessage = messages[Random.Shared.Next(messages.Length)];
        
        await SendAnimatedMessageAsync(recipientId, randomMessage);
        await Task.Delay(500);
        await SendMessageWithUrlButtonAsync(recipientId, "👆 Нажми, чтобы прочитать 💌", "💕 Открыть признание", link);
    }
    
    // Персонализированные сообщения для приглашений
    public async Task SendInviteNotificationAsync(int recipientId, int creatorId, string title, string description, DateTime date)
    {
        var recipient = GetUserInfo(recipientId);
        var creator = GetUserInfo(creatorId);
        
        var dateStr = date.ToString("dd MMMM, HH:mm", new System.Globalization.CultureInfo("ru-RU"));
        var descriptionLine = string.IsNullOrWhiteSpace(description) ? "" : $"\n<i>💭 {description}</i>";
        
        var message = creator.Gender == "male" 
            ? $"🎟️ <b>Приглашение от {creator.Name}!</b>\n\n💫 Он приглашает тебя на <b>«{title}»</b>{descriptionLine}\n\n🗓 <b>{dateStr}</b>\n\n✨ Загляни в раздел 'Приглашения' чтобы ответить!"
            : $"🎟️ <b>Приглашение от {creator.Name}!</b>\n\n💫 Она приглашает тебя на <b>«{title}»</b>{descriptionLine}\n\n🗓 <b>{dateStr}</b>\n\n✨ Загляни в раздел 'Приглашения' чтобы ответить!";
            
        await SendAnimatedMessageAsync(recipientId, message);
    }
    
    public async Task SendInviteConfirmationAsync(int creatorId, int recipientId, string title, DateTime date)
    {
        var creator = GetUserInfo(creatorId);
        var recipient = GetUserInfo(recipientId);
        
        var dateStr = date.ToString("dd MMMM, HH:mm", new System.Globalization.CultureInfo("ru-RU"));
        
        var message = creator.Gender == "male"
            ? $"🎉 <b>Приглашение отправлено!</b>\n\nТы пригласил <b>{recipient.Name}</b> на <b>«{title}»</b>\n\n🗓 <b>{dateStr}</b>\n\n💌 Она получила уведомление!"
            : $"🎉 <b>Приглашение отправлено!</b>\n\nТы пригласила <b>{recipient.Name}</b> на <b>«{title}»</b>\n\n🗓 <b>{dateStr}</b>\n\n💌 Он получил уведомление!";
            
        await SendMessageAsync(creatorId, message);
    }
    
    // Персонализированные сообщения для отмены приглашений
    public async Task SendInviteCancellationAsync(int recipientId, int creatorId, string title, DateTime date, string reason)
    {
        var recipient = GetUserInfo(recipientId);
        var creator = GetUserInfo(creatorId);
        
        var dateStr = date.ToString("dd MMMM, HH:mm", new System.Globalization.CultureInfo("ru-RU"));
        var reasonLine = string.IsNullOrWhiteSpace(reason) ? "без указания причины" : reason;
        
        var message = creator.Gender == "male"
            ? $"😔 <b>Изменение планов</b>\n\n{creator.Name} отменил приглашение на <b>«{title}»</b>\n🗓 <i>{dateStr}</i>\n\n💭 Причина: {reasonLine}\n\n💙 Не расстраивайся, обязательно встретитесь в другой раз!"
            : $"😔 <b>Изменение планов</b>\n\n{creator.Name} отменила приглашение на <b>«{title}»</b>\n🗓 <i>{dateStr}</i>\n\n💭 Причина: {reasonLine}\n\n💖 Не расстраивайся, обязательно встретитесь в другой раз!";
            
        await SendMessageAsync(recipientId, message);
    }
    
    public async Task SendInviteCancellationConfirmationAsync(int creatorId, int recipientId, string title)
    {
        var creator = GetUserInfo(creatorId);
        var recipient = GetUserInfo(recipientId);
        
        var message = creator.Gender == "male"
            ? $"✅ <b>Приглашение отменено</b>\n\nТы отменил <b>«{title}»</b>\n\n💌 {recipient.Name} получила уведомление об изменении планов"
            : $"✅ <b>Приглашение отменено</b>\n\nТы отменила <b>«{title}»</b>\n\n💌 {recipient.Name} получил уведомление об изменении планов";
            
        await SendMessageAsync(creatorId, message);
    }
    
    // Персонализированные сообщения для банка романтики
    public async Task SendExchangeRequestAsync(int providerId, int requesterId, string serviceName, string serviceEmoji, string? offeredServiceName = null, string? offeredServiceEmoji = null)
    {
        var provider = GetUserInfo(providerId);
        var requester = GetUserInfo(requesterId);
        
        var offerText = !string.IsNullOrEmpty(offeredServiceName) 
            ? $"\n\n💝 В обмен предлагает: {offeredServiceEmoji} <b>«{offeredServiceName}»</b>"
            : "";
            
        var message = requester.Gender == "male"
            ? $"💌 <b>Новый запрос на обмен!</b>\n\n{requester.Name} хочет получить:\n{serviceEmoji} <b>«{serviceName}»</b>{offerText}\n\n✨ Загляни в Банк Романтики чтобы ответить!"
            : $"💌 <b>Новый запрос на обмен!</b>\n\n{requester.Name} хочет получить:\n{serviceEmoji} <b>«{serviceName}»</b>{offerText}\n\n✨ Загляни в Банк Романтики чтобы ответить!";
            
        await SendAnimatedMessageAsync(providerId, message);
    }
    
    public async Task SendExchangeRequestConfirmationAsync(int requesterId, int providerId, string serviceName, string serviceEmoji, string? offeredServiceName = null, string? offeredServiceEmoji = null)
    {
        var requester = GetUserInfo(requesterId);
        var provider = GetUserInfo(providerId);
        
        var offerText = !string.IsNullOrEmpty(offeredServiceName) 
            ? $"\nВ обмен предложил: {offeredServiceEmoji} <b>«{offeredServiceName}»</b>"
            : "";
            
        var message = requester.Gender == "male"
            ? $"✨ <b>Запрос отправлен!</b>\n\nТы попросил у <b>{provider.Name}</b>:\n{serviceEmoji} <b>«{serviceName}»</b>{offerText}\n\n💌 Она получила уведомление!"
            : $"✨ <b>Запрос отправлен!</b>\n\nТы попросила у <b>{provider.Name}</b>:\n{serviceEmoji} <b>«{serviceName}»</b>{offerText}\n\n💌 Он получил уведомление!";
            
        await SendMessageAsync(requesterId, message);
    }
    
    public async Task SendExchangeAcceptedAsync(int requesterId, int providerId, string serviceName, string serviceEmoji, string? offeredServiceName = null, string? offeredServiceEmoji = null)
    {
        var requester = GetUserInfo(requesterId);
        var provider = GetUserInfo(providerId);
        
        var offerText = !string.IsNullOrEmpty(offeredServiceName) 
            ? $"\n💝 А взамен дарит: {offeredServiceEmoji} <b>«{offeredServiceName}»</b>"
            : "";
            
        var message = provider.Gender == "male"
            ? $"🎉 <b>Отличные новости!</b>\n\n{provider.Name} согласился на обмен!\n\n✅ Ты получишь: {serviceEmoji} <b>«{serviceName}»</b>{offerText}\n\n💕 Наслаждайтесь моментами вместе!"
            : $"🎉 <b>Отличные новости!</b>\n\n{provider.Name} согласилась на обмен!\n\n✅ Ты получишь: {serviceEmoji} <b>«{serviceName}»</b>{offerText}\n\n💕 Наслаждайтесь моментами вместе!";
            
        await SendAnimatedMessageAsync(requesterId, message);
    }
    
    public async Task SendExchangeRejectedAsync(int requesterId, string serviceName, string? reason = null)
    {
        var requester = GetUserInfo(requesterId);
        
        var reasonText = !string.IsNullOrEmpty(reason) ? $"\n\n💭 Причина: <i>{reason}</i>" : "";
        
        var message = $"😔 <b>Запрос не принят</b>\n\nК сожалению, запрос на <b>«{serviceName}»</b> был отклонён{reasonText}\n\n💙 Не расстраивайся, попробуй предложить что-то другое!";
        
        await SendMessageAsync(requesterId, message);
    }
    
    public async Task SendExchangeCompletedAsync(int providerId, int requesterId, string serviceName, string? offeredServiceName = null)
    {
        var provider = GetUserInfo(providerId);
        var requester = GetUserInfo(requesterId);
        
        var offerText = !string.IsNullOrEmpty(offeredServiceName) 
            ? $"\nВ обмен на <b>«{offeredServiceName}»</b>"
            : "";
            
        var message = $"✨ <b>Обмен завершён!</b>\n\n<b>«{serviceName}»</b> для {requester.Name}{offerText}\n\n🎉 Ещё один прекрасный момент в вашей истории любви!";
        
        await SendMessageAsync(providerId, message);
    }
    
    private UserInfo GetUserInfo(int userId)
    {
        return _userInfo.TryGetValue(userId, out var info) ? info : new UserInfo { Name = "Партнёр", Gender = "unknown", PartnerName = "Любимый" };
    }
    
    private class UserInfo
    {
        public string Name { get; set; } = "";
        public string Gender { get; set; } = ""; // "male", "female"
        public string PartnerName { get; set; } = "";
    }
}