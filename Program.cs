using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

namespace Диплом
{
    class Program
    {
        private static State state = State.NONE;
        private static string apiKey = "7bb80231d1143f692de8a08e6144c197";
        private static int max_overwiev_length = 500;
        private static SearchResult listrecommendation;
        private static string[] genres = new string[] { "Жахи", "Фентезі", "Бойовик", "Пригоди", "Мультфільм", "Комедія",
            "Кримінал", "Документальний", "Драма", "Сімейний", "Історичний", "Музика", "Детектив", "Мелодрама", "Фантастика",
            "Трилер", "Військовий", "Вестерн"};
        static async Task Main(string[] args)
        {
            // Запускаємо чат бота
            var botClient = new TelegramBotClient(token: "5356035802:AAHg2_pubDi4WRKqFv5mPsC8iLcJBpWQSQI");
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            botClient.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);
            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Запущен бот @{me.Username}");
            Console.ReadLine();
            cts.Cancel();

            static SearchResult GetSearch(string film_name)
            {
                //Метод який надсилає запит на отримання списку результатів із пошуку фільму
                string baseUri = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}";
                string language = "uk-UA";
                var sb = new StringBuilder(baseUri);

                sb.Append($"&language={language}");
                sb.Append($"&query={film_name}");
                sb.Append($"&page=1");

                var request = WebRequest.Create(sb.ToString());
                request.Timeout = 3000;
                request.Method = "GET";
                request.ContentType = "application/json";
                
                string result = string.Empty;

                try
                {
                    using (var response = request.GetResponse())
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                                var parsed_result = JsonConvert.DeserializeObject<SearchResult>(result);
                                return parsed_result;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine(result);
                return null;
            }

            static Movie CreateMovie(MovieResult movieResult)
            {
                // Заповнюємо дані знайденого фільму
                Movie movie = new Movie();
                movie.title = movieResult.title;
                movie.release_date = movieResult.release_date;
                movie.overview = movieResult.overview;
                movie.poster_path = movieResult.poster_path;
                movie.vote_average = movieResult.vote_average;
                List<Genre> genres = SearchGenres();
                List<string> genrenames = new List<string>();
                List<int> genre_ids = movieResult.genre_ids;
                if (genre_ids != null)
                {
                    for (int i = 0; i < genre_ids.Count; i++)
                    {
                        int currentGenreId = genre_ids[i];
                        for (int g = 0; g < genres.Count; g++)
                        {
                            Genre currentGenre = genres[g];
                            if (currentGenreId == currentGenre.id)
                            {
                                // Після пошуку жанрів по id отримуємо їх назви 
                                genrenames.Add(currentGenre.name);
                                break;
                            }
                        }
                    }
                }
                movie.genres = genrenames;
                return movie;
            }

            static List<Genre> SearchGenres()
            {
                // Метод який надсилає запит на отримання списку всіх жанрів
                string baseUri = $"https://api.themoviedb.org/3/genre/movie/list?api_key={apiKey}";
                string language = "uk-UA";

                var sb = new StringBuilder(baseUri);
                sb.Append($"&language={language}");

                var request = WebRequest.Create(sb.ToString());
                request.Timeout = 3000;
                request.Method = "GET";
                request.ContentType = "application/json";

                string result = string.Empty;

                try
                {
                    using (var response = request.GetResponse())
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                                var parsed_result = JsonConvert.DeserializeObject<GenresResult>(result);
                                return parsed_result.genres;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine(result);
                return null;
            }

            static int SearchGenreId(List<Genre> genres, string genrename)
            {
                // Метод для знаходження id жанру із списку жанрів
                if (genrename != null)
                {
                    for (int g = 0; g < genres.Count; g++)
                    {
                        Genre currentGenre = genres[g];
                        if (genrename == currentGenre.name)
                        {
                            return currentGenre.id;
                        }
                    }
                }
                return -1;
            }

            static SearchResult SearchRecommendation(int id)
            {
                // Метод який надсилає запит для пошуку списку фільмів обраного жанру
                string baseUri = $"https://api.themoviedb.org/3/discover/movie?api_key={apiKey}";
                string language = "uk-UA";
                string sort = "revenue.desc";

                var sb = new StringBuilder(baseUri);
                sb.Append($"&language={language}");
                sb.Append($"&sort_by={sort}");
                sb.Append($"&page=1");
                sb.Append($"&with_genres={id}");

                var request = WebRequest.Create(sb.ToString());
                request.Timeout = 3000;
                request.Method = "GET";
                request.ContentType = "application/json";

                string result = string.Empty;

                try
                {
                    using (var response = request.GetResponse())
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                                var parsed_result = JsonConvert.DeserializeObject<SearchResult>(result);
                                return parsed_result;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine(result);
                return null;
            }

            static bool IsMenuOption(string message)
            {
                // Метод для перевірки повідомлення після виводу таблиць
                if (message == "Вийти")
                {
                    return true;
                }
                if (message.Length <= 2)
                {
                    return false;
                }
                string filtered_name = message.Substring(2);
                for (int i = 0; i < genres.Length; i++)
                {
                    if (filtered_name == genres[i])
                    {
                        return true;
                    }
                }
                return false;
            }

            async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Метод для обробки оновлень в чаті
                if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                {
                    await HandleMessage(botClient, update.Message);
                    return;
                }
                if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallBackQuery(botClient, update.CallbackQuery);
                    return;
                }
            }

            async Task HandleMessage(ITelegramBotClient botClient, Message message)
            {
                // Метод для роботи з чатом 
                if (state == State.NONE)
                {
                    if (message.Text == "/start")
                    {
                        state = State.NONE;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: "Оберіть режим роботи: " +
                            "/search, /recommendation");
                    }
                    if (message.Text == "/search")
                    {
                        state = State.SEARCH;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: "Введіть назву фільму");
                    }
                    if (message.Text == "/recommendation")
                    {
                        state = State.RECOMENDATION;
                    }
                }
                else if (state == State.SEARCH)
                {
                    if (message.Text == "/stop")
                    {
                        state = State.NONE;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: "Натисніть на налаштування та " +
                                "виберіть видалити чат,перезапустити чат бота." +
                                "\nOберіть інший режим роботи: /search, /recommendation");
                    }
                    else
                    {
                        Console.WriteLine(message.Text);
                        var result = GetSearch(message.Text);

                        if (result != null && result.total_results > 0)
                        {
                            // Виводимо результат пошуку в чат
                            MovieResult movieResult = result.results[0];
                            Movie movie = CreateMovie(movieResult);
                            await botClient.SendPhotoAsync(message.Chat.Id,
                            photo: $"https://image.tmdb.org/t/p/w440_and_h660_bestv2{movie.poster_path}",
                            caption: $"<b>Назва:</b> {movie.title}\n" +
                            $"<b>Дата виходу:</b> {movie.release_date}\n" +
                            $"<b>Жанр:</b> {movie.GetGenreName()}\n" +
                            $"<b>Сюжет:</b> {movie.GetShortOverview(max_overwiev_length)}\n" +
                            $"<b>Оцінка:</b> {movie.vote_average} / 10\n",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                            await botClient.SendTextMessageAsync(message.Chat.Id, text: "Можете шукати фільми далі або напишіть /stop");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, text: "Напишіть коректну назву фільму!");
                        }
                    }
                }
                if (state == State.RECOMENDATION)
                {
                    if (message.Text == "/stop")
                    {
                        state = State.NONE;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: "Натисніть на налаштування та " +
                                "виберіть видалити чат,перезапустити чат бота." +
                                "\nOберіть інший режим роботи: /search, /recommendation");
                    }
                    else
                    {
                        if (message.Text == "/recommendation")
                        {
                            // Створюємо та виводимо таблицю жанрів
                            ReplyKeyboardMarkup recommendation = new(new[]
                            {
                                new KeyboardButton[] { "\U0001F383Жахи", "\U0001F52EФентезі", "\U0001F4A5Бойовик", "\U0001F392Пригоди" },
                                new KeyboardButton[] { "\U0001F466Мультфільм", "\U0001F602Комедія", "\U0001F52AКримінал",
                                    "\U0001F4F9Документальний" },
                                new KeyboardButton[] { "\U0001F622Драма", "\U0001F46AСімейний", "\U0001F3F0Історичний", "\U0001F3B6Музика" },
                                new KeyboardButton[] { "\U0001F50EДетектив", "\U0001F494Мелодрама", "\U0001F47DФантастика",
                                    "\U0001F628Трилер" },
                                new KeyboardButton[] { "\U0001F530Військовий", "\U0001F31FВестерн" },
                                new KeyboardButton[] { "Вийти" },
                            })
                            {
                                ResizeKeyboard = true, OneTimeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, text: "Оберіть жанр в меню",
                                replyMarkup: recommendation);
                        }
                        else
                        {
                            if (message.Text == "Вийти")
                            {
                                state = State.NONE;
                                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Натисніть на налаштування та " +
                                "виберіть видалити чат,перезапустити чат бота." +
                                "\nOберіть інший режим роботи: /search, /recommendation");
                            }
                            else
                            {
                                if (IsMenuOption(message.Text))
                                {
                                    // Користувач обрав жанр створюємо та виводимо таблицю фільмів в обраному жанрі
                                    string genrename = message.Text.Substring(2);
                                    List<Genre> genresResults = SearchGenres();
                                    int genre = SearchGenreId(genresResults, genrename);
                                    SearchResult recommendation = SearchRecommendation(genre);
                                    listrecommendation = recommendation;
                                    if (message.Text.EndsWith($"{genrename}"))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, text:
                                            $"Ось шість популярніших фільмів в жанрі {genrename}:");
                                        InlineKeyboardMarkup keyboard = new(new[]
                                        {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("1","0"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("2","1"),
                                        InlineKeyboardButton.WithCallbackData("3","2"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("4","3"),
                                        InlineKeyboardButton.WithCallbackData("5","4"),
                                        InlineKeyboardButton.WithCallbackData("6","5")
                                    }
                                });
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Оберіть фільм:", replyMarkup: keyboard);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Оберіть жанр фільму в меню!");
                                }
                            }
                        }
                    }
                }
            }

            async Task HandleCallBackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {
                // Метод для обробки результату обраного фільму в таблиці фільмів в обраному жанрі
                int choice = Convert.ToInt32(callbackQuery.Data);
                MovieResult movieResult = listrecommendation.results[choice];
                Movie movie = CreateMovie(movieResult);
                await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id,
                photo: $"https://image.tmdb.org/t/p/w440_and_h660_bestv2{movie.poster_path}",
                caption: $"<b>Назва:</b> {movie.title}\n" +
                $"<b>Дата виходу:</b> {movie.release_date}\n" +
                $"<b>Жанр:</b> {movie.GetGenreName()}\n" +
                $"<b>Сюжет:</b> {movie.GetShortOverview(max_overwiev_length)}\n" +
                $"<b>Оцінка:</b> {movie.vote_average} / 10\n",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: "Можете дивитися фільми далі або напишіть /stop");
                return;
            }

            Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
            {
                // Метод для виводу помилок з телеграму
                var ErrorMessag = exception switch
                {
                    ApiRequestException apiRequestException
                    => $"Помилка телеграм API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };
                Console.WriteLine(ErrorMessag);
                return Task.CompletedTask;
            }
        }
    }
    enum State
    {
        // Перелік можливих станів
        NONE,
        SEARCH,
        RECOMENDATION
    }
}
