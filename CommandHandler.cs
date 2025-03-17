using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace Discord_Bot_Dusk
{
    /// <summary>
    /// Commands Functionality Occurs Here.
    /// </summary>
    public class CommandHandler
    {
        private static TimeZones TimeZone;
        private static readonly string? devId = Environment.GetEnvironmentVariable("FatherId");
        private static DiscordSocketClient? _client;
        private static readonly HttpClient _httpClient = new HttpClient();
        // Add a simple cache system
        private static readonly Dictionary<string, (DateTime Expiry, string Data)> _amiiboCache = new();
        private static readonly Dictionary<TimeZones, TimeZoneInfo> _timeZoneInfos = new()
        {
            { TimeZones.EST, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { TimeZones.PST, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { TimeZones.CST, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { TimeZones.MST, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time") },
            { TimeZones.AKST, TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time") },
            { TimeZones.HST, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") }
        };

        /// <summary>
        /// Initialize the Command Handler
        /// </summary>
        /// <param name="client"></param>
        public static void Initialize(DiscordSocketClient client)
        {
            _client = client;
        }

        /// <summary>
        /// This method handles the command that was sent by the user. Aka The Place where functionality of the commands are implemented.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static async Task HandleCommand(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "current-time":
                    var option = command.Data.Options.FirstOrDefault(o => o.Name == "timezones");

    #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    string userTimeZone = option.Value.ToString().ToUpper(); // Normalize case
    #pragma warning restore CS8602 // Dereference of a possibly null reference.


                    // Get current UTC time and convert to the selected time zone
                    DateTime currentTime = DateTime.UtcNow;
                    // Convert using enum instead of string comparison
                    TimeZone = Enum.Parse<TimeZones>(userTimeZone);

                    string formattedTime = TimeZone switch
                    {
                        TimeZones.HST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time")).ToString("hh:mm tt"),
                        TimeZones.AKST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time")).ToString("hh:mm tt"),
                        TimeZones.PST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")).ToString("hh:mm tt"),
                        TimeZones.MST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).ToString("hh:mm tt"),
                        TimeZones.CST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")).ToString("hh:mm tt"),
                        TimeZones.EST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).ToString("hh:mm tt"),
                        _ => currentTime.ToString("hh:mm tt") // Default case (use UTC)
                    };

                    await command.RespondAsync($"Currently it is {formattedTime} in {userTimeZone} {command.User.Mention}"); // Only returns the time
                    return;
                case "make-sandwich":
                    Dictionary<int, string> sandwiches = new()
                        {
                            {0, "Tuna"}, {1, "Chicken"}, {2, "Turkey"}, {3, "Beef"},
                            {4, "Ham"}, {5, "Pastrami"}, {6, "BLT"}, {7, "Club"},
                            {8, "Grilled Cheese"}, {9, "PB&J"}, {10, "Egg Salad"},
                            {11, "Roast Beef"}, {12, "Italian"}, {13, "Veggie"},
                            {14, "Reuben"}, {15, "French Dip"}, {16, "Meatball"},
                            {17, "Pulled Pork"}, {18, "Cuban"}, {19, "Caprese"},
                            {20, "Philly Cheesesteak"},{21, "Monte Cristo"}, {22, "Croque Monsieur"},
                            {23, "Hot Dog"},{24,"Lobster Roll"}
                        };
                    Random random = new Random();
                    int randomIndex = random.Next(sandwiches.Count);
                    var responseMessage = sandwiches[randomIndex];
                    if(randomIndex == 23){
                       await command.RespondAsync($"{command.User.Mention} Here is your hot dog (it's like a sandwich trust). Enjoy!");
                    }
                    await command.RespondAsync($"{command.User.Mention} Here is your {responseMessage} sandwich.");
                    break;
                case "current-day":
                    var optionMore = command.Data.Options.FirstOrDefault(o => o.Name == "timezones");
                    string? userTimeZone2 = optionMore?.Value?.ToString()?.ToUpper();
                    if (string.IsNullOrEmpty(userTimeZone2))
                    {
                        await command.RespondAsync($"{DateTime.Today:dddd, MMMM dd yyyy} is the current day. {command.User.Mention}");
                        return;
                    }

                    // Convert from UTC to the given time zone
                    DateTime utcNow = DateTime.UtcNow;
                    if (!Enum.TryParse(userTimeZone2, out TimeZones timezone))
                    {
                        await command.RespondAsync("Other TimeZones are WIP. Please use US-Based TimeZones (EST, CST, MST, PST, AKST, HST.)", ephemeral: true);
                        return;
                    }

                    TimeZoneInfo timeZone = timezone switch
                    {
                        TimeZones.EST => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
                        TimeZones.PST => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
                        TimeZones.CST => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
                        TimeZones.MST => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
                        TimeZones.AKST => TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"),
                        TimeZones.HST => TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"),
                        _ => TimeZoneInfo.Local // Default case (use local timezone)
                    };

                    DateTime userTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

                    // Send response
                    await command.RespondAsync($"{userTime:dddd, MMMM dd yyyy} is the current day in {userTimeZone2}.");
                    return;
                case "hello-son":
                    try
                    {
                        if (devId != null && ulong.Parse(devId) == command.User.Id)
                        {
                            await command.RespondAsync("Greetings, how may I assist you father?");
                        }
                        else
                        {
                            await command.RespondAsync($"Access Denied!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in hello-son command: {ex.Message}");
                    }
                    return;
                case "boom":
                    if (devId != null && ulong.Parse(devId) != command.User.Id)
                    {
                        await command.RespondAsync("# 💥 KABOOM 💥 #");
                        await command.FollowupAsync($"{command.User.Mention} has blown themselves up");
                    }
                    else
                    {
                        await command.RespondAsync("I'm sorry, I can't let you do that father.");
                        await Task.Delay(1000);
                        await command.FollowupAsync("But here is 5 big booms");
                        for (int i = 0; i < 5; i++)
                        {
                            await command.FollowupAsync("# 💥 BOOM 💥 #");
                            await Task.Delay(500);
                        }
                    }
                    return;
                case "set-timer":
                    var optionTimer = command.Data.Options.FirstOrDefault(o => o.Name == "time");
                    if (optionTimer?.Value == null)
                    {
                        await command.RespondAsync("Please provide a valid time to set the timer for.");
                        return;
                    }

                    string? timeStr = optionTimer.Value.ToString();
                    if (string.IsNullOrEmpty(timeStr))
                    {
                        await command.RespondAsync("Please provide a valid time to set the timer for.");
                        return;
                    }
                    int time = int.Parse(timeStr);
                    if (time < 0)
                    {
                        await command.RespondAsync("Please provide a valid time to set the timer for.");
                        return;
                    }

                    await command.RespondAsync($"Timer set for {time} seconds.");
                    await Task.Delay(time * 1000);
                    await command.FollowupAsync($"{command.User.Mention} Your timer has ended.");
                    return;
                case "set-reminder":
                    var optionDate = command.Data.Options.FirstOrDefault(o => o.Name == "date");
                    var optionMessage = command.Data.Options.FirstOrDefault(o => o.Name == "message");
                    var optionTimeZone = command.Data.Options.FirstOrDefault(o => o.Name == "timezones");

                    if (optionDate?.Value == null || optionMessage?.Value == null)
                    {
                        await command.RespondAsync("Please provide a valid date and message to set the reminder for.");
                        return;

                    }
                    else
                    {
                        string? dateStr = optionDate.Value.ToString();
                        string? message = optionMessage.Value.ToString();
                        string? timeZoneStr = optionTimeZone?.Value?.ToString()?.ToUpper();

                        if (string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(message))
                        {
                            await command.RespondAsync("Please provide a valid date and message to set the reminder for.");
                            return;
                        }

                        DateTime date = DateTime.Parse(dateStr);
                        // Parse the timezone string to enum
                        if (!Enum.TryParse(timeZoneStr, out TimeZones parsedTimezone))
                        {
                            parsedTimezone = TimeZones.EST; // Default to EST if parsing fails
                        }

                        TimeZoneInfo timeZone2 = _timeZoneInfos.TryGetValue(parsedTimezone, out var tz) 
                            ? tz 
                            : TimeZoneInfo.Local;

                        DateTime userDate = TimeZoneInfo.ConvertTime(date, timeZone2);

                        // Send response
                        await command.RespondAsync($"{command.User.Mention} Reminder set for {userDate:dddd, MMMM dd yyyy} at {userDate:hh:mm tt}.");
                        await Task.Delay((int)(userDate - DateTime.UtcNow).TotalMilliseconds);
                        await command.FollowupAsync($"{command.User.Mention} Reminder: {message}");
                    }
                    return;

                case "delete-command":
                    var optionDelete = command.Data.Options.FirstOrDefault(o => o.Name == "message-id");
                    if (optionDelete?.Value == null)
                        return;
                    string? messageId = optionDelete.Value.ToString();
                    if (string.IsNullOrEmpty(messageId))
                        return;

                    if (devId == null)
                    {
                        await command.RespondAsync("Developer ID not found.");
                        return;
                    }

                    if (command.User.Id == ulong.Parse(devId) && !command.User.IsBot)
                    {
                        var message = await command.Channel.GetMessageAsync(ulong.Parse(messageId));
                        if (message != null)
                        {
                            await message.DeleteAsync();
                            await command.RespondAsync("Message deleted successfully.");
                        }
                        else
                        {
                            await command.RespondAsync("Message not found.");
                        }
                    }
                    else
                    {
                        await command.RespondAsync("You do not have permission to delete messages.");
                    }

                    return;
                case "roulette":
                    var optionBullets = command.Data.Options.FirstOrDefault(o => o.Name == "bullets");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    string? bulletsStr = optionBullets.Value.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    if (optionBullets?.Value == null || string.IsNullOrEmpty(bulletsStr))
                    {
                        await command.RespondAsync("Please provide the number of bullets to load.");
                        return;
                    }
                    else
                    {
                        int bullets = int.Parse(bulletsStr);
                        if (bullets < 1 || bullets > 6)
                        {
                            await command.RespondAsync("Please provide a number of bullets between 1 and 6.");
                            return;
                        }

                        Random rand = new Random();
                        var minRange = 1;
                        var maxRange = 6; // Change to 6 to represent a standard revolver
                        int chamber = rand.Next(minRange, maxRange + 1); // +1 because Next is exclusive on upper bound
                        if (chamber <= bullets)
                        {
                            await command.RespondAsync("Click... 💥");
                            await command.FollowupAsync($"{command.User.Mention} has been shot dead.");
                        }
                        else
                        {
                            await command.RespondAsync("Click... 💥");
                            await command.FollowupAsync($"{command.User.Mention} has survived.");
                        }
                    }
                    return;
                case "math":
                    var optionOperation = command.Data.Options.FirstOrDefault(o => o.Name == "operation");
                    var optionNum1 = command.Data.Options.FirstOrDefault(o => o.Name == "num1");
                    var optionNum2 = command.Data.Options.FirstOrDefault(o => o.Name == "num2");

                    if (optionOperation?.Value == null || optionNum1?.Value == null || optionNum2?.Value == null)
                    {
                        await command.RespondAsync("Please provide a valid operation and two numbers to perform the operation on.");
                        return;
                    }
                    else
                    {
                        string? operation = optionOperation.Value.ToString();
                        string? num1Str = optionNum1.Value.ToString();
                        string? num2Str = optionNum2.Value.ToString();

                        if (string.IsNullOrEmpty(operation) || string.IsNullOrEmpty(num1Str) || string.IsNullOrEmpty(num2Str))
                        {
                            await command.RespondAsync("Please provide a valid operation and two numbers to perform the operation on.");
                            return;
                        }

                        double num1 = double.Parse(num1Str);
                        double num2 = double.Parse(num2Str);
                        double result = operation.ToLower() switch
                        {
                            "add" => Math.Calculate(num1, num2, Math.Operations.Add),
                            "subtract" => Math.Calculate(num1, num2, Math.Operations.Subtract),
                            "multiply" => Math.Calculate(num1, num2, Math.Operations.Multiply),
                            "divide" => TryDivide(num1, num2),
                            _ => double.NaN // Default case for unknown operations
                        };

                        double TryDivide(double a, double b)
                        {
                            try { return Math.Calculate(a, b, Math.Operations.Divide); }
                            catch (DivideByZeroException) { return double.NaN; }
                        }

                        string resultStr = double.IsNaN(result) ? "Cannot divide by zero." : result.ToString();
                        await command.RespondAsync($"{command.User.Mention} The result is {resultStr}");
                    }
                    return;
                case "joke":
                    string url = "https://sv443.net/jokeapi/v2/joke/Any";
                    try
                    {
                        // Set a reasonable timeout
                        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
                        string json = await _httpClient.GetStringAsync(url, cancellationToken);
                        JObject joke = JObject.Parse(json);

                        if (joke["type"]?.ToString() == "twopart")
                        {
                            string setup = joke["setup"]?.ToString() ?? "No setup available";
                            string delivery = joke["delivery"]?.ToString() ?? "No delivery available";
                            string jokeText = setup + "\n" + delivery;
                            await command.RespondAsync(jokeText);
                        }
                        else // Handle single part jokes
                        {
                            string jokeText = joke["joke"]?.ToString() ?? "No joke available";
                            await command.RespondAsync(jokeText);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in joke command: {ex.Message}");
                        await command.RespondAsync("An error occurred while fetching the joke.");
                    }
                    return;
                case "amiibo":
                    var optionID = command.Data.Options.FirstOrDefault(o => o.Name == "name");  
                    var optionValue = optionID?.Value.ToString();
                    if (string.IsNullOrEmpty(optionValue))
                    {
                        await command.RespondAsync("Please provide a valid amiibo name.");
                        return;
                    }
                    else{
                        // Check cache first
                        string cacheKey = optionValue.ToLowerInvariant();
                        if (_amiiboCache.TryGetValue(cacheKey, out var cachedData) && cachedData.Expiry > DateTime.UtcNow)
                        {
                            // Use cached data directly
                            JObject amiiboJson = JObject.Parse(cachedData.Data);
                            // Process the JSON...
                            return;
                        }
                        
                        string id = optionValue.ToString();
                        string url2 = $"https://amiiboapi.com/api/amiibo?name={id}";
                        using (HttpClient client = new HttpClient())
                        {
                            try
                            {
                                string json = await client.GetStringAsync(url2);
                                // Cache the result (expires in 24 hours)
                                _amiiboCache[cacheKey] = (DateTime.UtcNow.AddHours(24), json);
                                JObject amiiboJson = JObject.Parse(json);
                                JArray? amiiboArray = amiiboJson["amiibo"] as JArray;
                                
                                if (amiiboArray == null || !amiiboArray.Any())
                                {
                                    await command.RespondAsync("No amiibo information found.",ephemeral:true);
                                    return;
                                }
                                
                                JToken amiibo = amiiboArray[0];
                                string name = amiibo["name"]?.ToString() ?? "No name available";
                                string series = amiibo["amiiboSeries"]?.ToString() ?? "No series available";
                                string character = amiibo["character"]?.ToString() ?? "No character available";
                                string gameSeries = amiibo["gameSeries"]?.ToString() ?? "No game series available";
                                string image = amiibo["image"]?.ToString() ?? "No image available";
                                var embed = new EmbedBuilder()
                                .WithTitle(name)
                                .WithDescription($"Character: {character}\nSeries: {series}\nGame Series: {gameSeries}")
                                .WithImageUrl(image)
                                .WithColor(Color.Blue)
                                .Build();
                                await command.RespondAsync(embed: embed);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error in amiibo command: {ex.Message}");
                                await command.RespondAsync("An error occurred while fetching the amiibo information.");
                            }
                        }
                    }
                    return;
                
            }
        }
    }
}