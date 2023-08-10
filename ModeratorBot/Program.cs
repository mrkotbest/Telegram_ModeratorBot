using Microsoft.VisualBasic;
using ModeratorBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string TELEGRAM_BOT_TOKEN = "6177939262:AAHIaPrlRVOu4aCh15EmE7Sqg-g4k_FG99M";
const long ADMIN_ID = 438889695;

var botClient = new TelegramBotClient(TELEGRAM_BOT_TOKEN);

using var cts = new CancellationTokenSource();

var huis = new List<Hui>();
var words = new List<string>
{
    "Хуй",
    "Пизда",
    "Тупорыл",
    "Шлюха",
    "Мразь",
    "Еблан",
    "Гондон",
    "Хуесос",
    "Шалава",
    "Уебок",
    "Тварь",
    "Сука",
    "Блядина",
    "Шмара",
    "Мудак",
    "Пиздолиз",
    "Ебать",
    "Охуеть",
    "Выблядок",
    "Заебал",
    "Лох",
    "Нихуя",
    "Отсоси",
    "Пососи",
    "Жопу"
};

var receivedOptions = new ReceiverOptions() { AllowedUpdates = { } };

// Bot is started
botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receivedOptions, cts.Token);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Starting with @{me.Username}");

await Task.Delay(int.MaxValue);

cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message || update.Type == UpdateType.EditedMessage)
    {
        var chatId = update.Message is null ? update.EditedMessage!.Chat!.Id : update.Message!.Chat!.Id;
        var messageText = update.Message is null ? update.EditedMessage!.Text : update.Message!.Text;
        var messageId = update.Message is null ? update.EditedMessage!.MessageId : update.Message!.MessageId;
        var fromId = update.Message is null ? update.EditedMessage!.From!.Id : update.Message!.From!.Id;

        var username = update.Message is null ? update.EditedMessage!.From!.Username ?? $"{default}" : update.Message!.From!.Username ?? $"{default}";
        
        if (fromId == ADMIN_ID) return;

        var countOfBans = huis.Where(h => h.ID == fromId).Count() + 1;
        
        foreach (var item in words)
        {
            if (messageText != null && messageText.Contains(item, StringComparison.OrdinalIgnoreCase))
            {
                // Delete Criminal Message
                await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);

                // Restrict Chat for User
                await botClient.RestrictChatMemberAsync(
                    chatId: chatId,
                    userId: fromId,
                    permissions: new ChatPermissions()
                    {
                        CanInviteUsers = false,
                        CanPinMessages = false,
                        CanAddWebPagePreviews = false,
                        CanChangeInfo = false,
                        CanSendMediaMessages = false,
                        CanSendPolls = false,
                        CanSendOtherMessages = false,
                        CanSendMessages = false
                    },
                    untilDate: DateTime.Now.AddSeconds(15 * countOfBans),
                    cancellationToken: cancellationToken);

                // Send Ban Message
                double timeOfBan = (15.0 * countOfBans);
                string interrupt = $"Не стоит материться в чате @{ username }\nВы забанены на срок → <code>{ timeOfBan } сек</code>";
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: interrupt,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                // Add the Criminal to List
                huis.Add(new Hui() { ID = fromId });
                Console.WriteLine($"New_Hui_ID #{ fromId } | Username: @{ username } | Count_Of_Bans: { countOfBans } | Message: { messageText }");
            }
        }
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}