using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace KlondaikLyubvi.Services;

public class TelegramService
{
    private readonly TelegramBotClient _botClient = new(BotToken);
    // Для примера: токен и chatId можно вынести в настройки
    private const string BotToken = "8303606158:AAG3L8iV0iZvNMeKFvnBdn5Ma_vCC8K-pdI";
    private readonly Dictionary<int, long> _userChatIds = new()
    {
        { 1, 721196467 }, // Denis
        { 2, 260558500 }  // Liza   
    };

    public async Task SendMessageAsync(int userId, string message)
    {
        if (_userChatIds.TryGetValue(userId, out var chatId))
        {
            try
            {
                await _botClient.SendMessage(chatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                // Не падаем из-за сетевых/Telegram ошибок в проде
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
                await _botClient.SendMessage(chatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: markup);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telegram] Send failed: {ex.Message}");
            }
        }
    }
}