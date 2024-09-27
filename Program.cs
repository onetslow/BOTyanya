using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Telegram.Bot.Args;
using Telegram.Bot.Requests;
using System.Timers;
using Telegram.Bot.Types.Enums;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class Program
{
    private static TelegramBotClient client;
    private static bool isRun = true;
    private static bool isPredictionRequested = false;
    /////
    private static string userChatId;
    private static string userTimeZone;
    private static System.Timers.Timer predictionTimer;
    private static Dictionary<string, System.Timers.Timer> userTimers = new Dictionary<string, System.Timers.Timer>();
    /////

    static async Task Main(string[] args)
    {
        client = new TelegramBotClient("7966740995:AAGxmN_5XbuSjUxpBPqqVK91qjYkPG1i4IM"); // 7231708809:AAHWRPtt30pc7FHouUQ-cIaFKVcMgVgnMR8 - запаска  7433969050:AAGGjFBsmNI2VuGiFqOShpq7Jbn2SYdP570 - помер
        var cancellationTokenSource = new CancellationTokenSource();
        client.StartReceiving(Update, Error);
        while (isRun)
        {
            Console.ReadLine();
        }
    }

    static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var message = update.Message;
  

        if (message.Text == "/start")
        {
            isRun = true;
            await botClient.SendTextMessageAsync(message.Chat, "Здравствуй, товарищ! Это Владимир Вольфович, и я знаю, как вывести тебя на путь к успеху!\nХочешь узнать, что ждет тебя сегодня? Спрашивай меня!", replyMarkup: GetButtons());
            Console.WriteLine($"{message.Chat.Username ?? "аноним"}    |    Старт");
        }
        else if (message.Text == "/end")
        {
            isRun = false;
            await botClient.SendTextMessageAsync(message.Chat, "Ну все, не отвлекаю! Но помни: Жириновский всегда прав! Возвращайся, как заскучаешь!");
            Console.WriteLine($"{message.Chat.Username ?? "аноним"}    |    Финиш");
        }
        else if (message.Text == "/regular_off")
        {
            string userChatId = message.Chat.Id.ToString();

            if (userTimers.ContainsKey(userChatId) && userTimers[userChatId].Enabled)
            {
                userTimers[userChatId].Stop();
                userTimers[userChatId].Dispose();
                userTimers.Remove(userChatId);
                isPredictionRequested = false;

                await botClient.SendTextMessageAsync(message.Chat, "Регулярные предсказания отключены.");
                Console.WriteLine($"{message.Chat.Username ?? "аноним"} | Удалил таймер");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, "У вас нет активных регулярных предсказаний.");
            }
        }

        else if (isRun != false && message.Text != null && message.Text != "/end")
        {
            Console.WriteLine($"{message.Chat.Username ?? "аноним"}    |    {message.Text}");

            switch (message.Text)
            {
                case "Получить предсказание": //название кейса д.б. как у кнопки                            
                                              //string randomMessage = GetRandomMessage();                                                              //это обычное неограниченное предсказание
                                              //await botClient.SendTextMessageAsync(message.Chat.Id, randomMessage, cancellationToken: token);
                                              //break;
                    if (isPredictionRequested) //это с ограничением и ворчанием на несколько кликов предсказаний
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ещё одно? Про жадность фраера слышали? \n\nОни все по своей жадности никак не поймут, что скоро за эту бумажку не то что рубля не дадут — а дадут просто в морду!", cancellationToken: token);
                        var replyMarkup = GetButtons();
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ладно, жди уж, сейчас придумаю...", replyMarkup: replyMarkup, cancellationToken: token);
                        await Task.Delay(3000);
                        string randomMessage = GetRandomMessage();
                        await botClient.SendTextMessageAsync(message.Chat.Id, randomMessage, cancellationToken: token);
                        isPredictionRequested = false; // Сбрасываем флаг после отправки предсказания
                    }
                    else
                    {
                        isPredictionRequested = true;
                        string randomMessage = GetRandomMessage();
                        await botClient.SendTextMessageAsync(message.Chat.Id, randomMessage, cancellationToken: token);
                    }
                    break;
                case "Факты обо мне (не о тебе, товарищ)":
                    await botClient.SendTextMessageAsync(message.Chat, "Открываю Жирипедию...");
                    string randomFact = GetRandomFact();
                    await botClient.SendTextMessageAsync(message.Chat, randomFact);
                    break;
                case "АнЕкДоТ":
                    await botClient.SendTextMessageAsync(message.Chat, "Внимание АнЕкДоТ:");
                    string randomAnecdote = GetRandomAnecdote();
                    await botClient.SendTextMessageAsync(message.Chat, randomAnecdote);
                    break;
                case "Жириновский LIVE":
                    await botClient.SendTextMessageAsync(message.Chat, "Сейчас покажу тебе интересный эпизод с Жириновским!");
                    string videoPath = GetRandomVideo();

                    if (videoPath.StartsWith("В папке нет") || videoPath.StartsWith("Произошла ошибка"))
                    {
                        await botClient.SendTextMessageAsync(message.Chat, videoPath); // Отправляем сообщение об ошибке
                    }
                    else
                    {
                        try
                        {
                            using (var stream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                // Создаем объект InputOnlineFile, который принимает Stream
                                var inputFile = new InputFileStream(stream, Path.GetFileName(videoPath));
                                await botClient.SendVideoAsync(message.Chat.Id, inputFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Ошибка при отправке видео: {ex.Message}"); // Отправляем сообщение об ошибке
                        }
                    }
                    break;

                ////////////////////////////////////////////////////////*************
                case "Регулярные предсказания":
                    await botClient.SendTextMessageAsync(message.Chat, "Регулярно, товарищ? Готовься к шокирующим истинам!");
                    string chatId = message.Chat.Id.ToString();
                    if (userTimers.ContainsKey(chatId) && userTimers[chatId].Enabled)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, " Много хочешь, товарищ. У тебя уже есть установленный таймер - удали его перед настройкой нового.");
                        break; // Останавливаем дальнейшую обработку
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Укажи время в формате ЧЧ:ММ (например, 14:30).");
                        isPredictionRequested = true; // Устанавливаем индикатор, что ожидаем ввод времени
                        return; // Пауза для ожидания времени
                    }
                default:
                    // Проверяем, если ожидается ввод времени
                    if (isPredictionRequested)
                    {
                        if (IsValidTimeFormat(message.Text))
                        {
                            SetPredictionTimer(message.Text, message.Chat.Id.ToString());
                            await botClient.SendTextMessageAsync(message.Chat, $"Предсказания будут приходить каждый день в {message.Text} по твоему времени!");
                            isPredictionRequested = false; // Сбрасываем флаг после установки времени
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Неверный формат времени, присылал же пример, товарищ. Попробуй еще раз в формате ЧЧ:ММ.");
                        }
                    }
                    else
                    {
                        // Обработка неверных команд
                        await botClient.SendTextMessageAsync(message.Chat, "Командуй, а не лясы точи!", replyMarkup: GetButtons());
                    }
                    break;
            }
        }
    }

    ////////////////////////////////
    private static void SetPredictionTimer(string time, string userChatId)
    {
        TimeSpan scheduledTime = TimeSpan.Parse(time);
        DateTime now = DateTime.Now;

        if (now.TimeOfDay > scheduledTime)
        {
            scheduledTime = scheduledTime.Add(TimeSpan.FromDays(1));
        }

        double timeUntilSend = (scheduledTime - now.TimeOfDay).TotalMilliseconds;

        var predictionTimer = new System.Timers.Timer(timeUntilSend);
        predictionTimer.Elapsed += async (sender, e) =>
        {
            await client.SendTextMessageAsync(long.Parse(userChatId), GetRandomMessage());
            predictionTimer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
            predictionTimer.Start();
        };

        predictionTimer.AutoReset = false; // Одноразовый запуск
        predictionTimer.Start();

        // Сохраняем таймер в словаре
        userTimers[userChatId] = predictionTimer;

        Console.WriteLine($"Таймер установлен для пользователя {userChatId} на {time}");
    }

    private static async Task<string> GetUserTimezone(ITelegramBotClient botClient, Telegram.Bot.Types.Message message) 
    {
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync("http://api.ipinfodb.com/v3/ip-city/?key=YOUR_API_KEY&ip=YOUR_IP_ADDRESS&format=json");
            response.EnsureSuccessStatusCode();
            var timezoneInfo = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(timezoneInfo);
            return jsonObject["timezone"];
        }
    }

    static bool IsValidTimeFormat(string time)
    {
        return Regex.IsMatch(time, @"^(2[0-3]|[01]?[0-9]):[0-5][0-9]$");
    }

    ////////////////////////////////////


    private static string GetRandomVideo() 
    {
        string videoFolderPath = @"Video\"; 
        try
        {
            string[] videoFiles = Directory.GetFiles(videoFolderPath, "*.mp4"); // Здесь можно указать другие форматы, если нужно
            if (videoFiles.Length > 0)
            {
                Random random = new Random();
                int index = random.Next(videoFiles.Length);
                return videoFiles[index]; // Возвращаем полный путь к видео
            }
            else
            {
                return "В папке нет видеофайлов.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при доступе к папке: {ex.Message}");
            return "Произошла ошибка при доступе к папке.";
        }
    }
    ////////////////////////////**************************
    private static string GetRandomMessage()
    {
        string filePath = "predictions.txt"; //фактический путь к файлу
        //в копировку
        try
        {
            string[] messages = System.IO.File.ReadAllLines(filePath);   
            if (messages.Length > 0)
            {
                Random random = new Random();
                int index = random.Next(messages.Length);
                return messages[index];
            }
            else
            {
                return "Файл пуст или не найден.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            return "Произошла ошибка при чтении файла.";
        }
    }

    private static string GetRandomFact()
    {
        string filePath = "facts.txt";

        try
        {
            string[] facts = System.IO.File.ReadAllLines(filePath);
            if (facts.Length > 0)
            {
                Random random = new Random();
                int index = random.Next(facts.Length);
                return facts[index];
            }
            else
            {
                return "Файл пуст или не найден.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            return "Произошла ошибка при чтении файла.";
        }
    }

    private static string GetRandomAnecdote()
    {
        string filePath = "anecdotes.txt";

        try
        {
            string[] anecdotes = System.IO.File.ReadAllLines(filePath);
            if (anecdotes.Length > 0)
            {
                Random random = new Random();
                int index = random.Next(anecdotes.Length);
                return anecdotes[index];
            }
            else
            {
                return "Файл пуст или не найден.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            return "Произошла ошибка при чтении файла.";
        }
    }

    private static async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private static IReplyMarkup GetButtons()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
                        new[] { new KeyboardButton("Получить предсказание"), new KeyboardButton("Факты обо мне (не о тебе, товарищ)") },
                        new[] { new KeyboardButton("АнЕкДоТ"), new KeyboardButton("Жириновский LIVE") }, ////////////////////////////**************************
                        new[] { new KeyboardButton("Регулярные предсказания") }
            }
        );
    }
}

