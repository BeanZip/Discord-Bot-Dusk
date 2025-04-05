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
            { TimeZones.HST, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { TimeZones.JST, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")},
            { TimeZones.AEST, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time")},
            {TimeZones.NZST, TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time")}
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
        {   try{
                   Console.WriteLine($"[{DateTime.UtcNow}], {command.Data.Name} used by {command.User.ToString()} in {command.GuildId.ToString()}");
               } catch(Exception ex){
                   Console.WriteLine($"Couldn't Log Message at {DateTime.UtcNow} because of {ex.Message}");
               }

            switch (command.Data.Name)
            {
                case "current-time":
                    var option = command.Data.Options.FirstOrDefault(o => o.Name == "timezones");
                     string userTimeZone = option.Value.ToString().ToUpper(); // Normalize case
                    // Get current UTC time and convert to the selected time zone
                    DateTime currentTime = DateTime.UtcNow;
                    // Convert using enum instead of string comparison
                    TimeZone = Enum.Parse<TimeZones>(userTimeZone);
                    if(userTimeZone == null){
                      await command.RespondAsync("Time Zone Variable isn't Set");
                    }

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
                        TimeZones.JST => TimeZoneInfo.ConvertTimeFromUtc(currentTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString("hh:mm tt"),
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
                            {23, "Hot Dog"},{24,"Lobster Roll"},{25,"[REDACTED]"}
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
                        TimeZones.JST => TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"),
                        TimeZones.AEST => TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"),
                        TimeZones.NZST => TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time"),
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
                    NotImplementedException exo = new NotImplementedException();
                    await command.RespondAsync($"This command is not implemented yet. {exo.Message}");
                    return;
                case "roulette":
                    var optionBullets = command.Data.Options.FirstOrDefault(o => o.Name == "bullets");
                    if(optionBullets == null || optionBullets.Value == null) {
                        await command.RespondAsync("Please provide the number of bullets to load.");
                        return;
                    }

                    string? bulletsStr = optionBullets.Value.ToString();

                    if (string.IsNullOrEmpty(bulletsStr))
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
                        if(num2 == 0){
                          await command.FollowupAsync("Only use sqrt or power with one value operations");
                        }
                        double result = operation.ToLower() switch
                        {
                            "add" => Math.Calculate(num1, num2, Math.Operations.Add),
                            "subtract" => Math.Calculate(num1, num2, Math.Operations.Subtract),
                            "multiply" => Math.Calculate(num1, num2, Math.Operations.Multiply),
                            "divide" => TryDivide(num1, num2),
                            "power" => Math.Calculate(num1,num2,Math.Operations.Power),
                            "sqrt" => Math.Calculate(num1, num2, Math.Operations.Sqrt),
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
                   var id = command.Data.Options.FirstOrDefault(o => o.Name == "id");
                   if(id == null){
                     await command.RespondAsync("Please add a value");
                   }
                   var idStr = id?.Value.ToString();
                   string url2 = "https://dusk-amiibo-backend-production.up.railway.app/amiibo/id=";
                   try{
                     var cancellationToken2 = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
                     string json = await _httpClient.GetStringAsync(url2 + idStr ,cancellationToken2);
                     JObject amiibo = JObject.Parse(json);
                     string name = amiibo["name"]?.ToString() ?? "No amiibo name available?? (That's weird)";
                     string series = amiibo["series"]?.ToString() ?? "No Series Available (Bro does this amiibo exist)";

                     // Access nested release date information
                     string usReleaseDate = "N/A";
                     string euReleaseDate = "N/A";
                     string jpReleaseDate = "N/A";

                     // Handle US release date
                     if (amiibo["release"]?["us"] != null)
                     {
                         int? month = (int?)amiibo["release"]?["us"]?["month"];
                         int? day = (int?)amiibo["release"]?["us"]?["day"];
                         int? year = (int?)amiibo["release"]?["us"]?["year"];
                         
                         if (month.HasValue && day.HasValue && year.HasValue)
                         {
                             usReleaseDate = $"{month.Value}/{day.Value}/{year.Value}";
                         }
                     }

                     // Handle EU release date
                     if (amiibo["release"]?["eu"] != null)
                     {
                         int? month = (int?)amiibo["release"]?["eu"]?["month"];
                         int? day = (int?)amiibo["release"]?["eu"]?["day"];
                         int? year = (int?)amiibo["release"]?["eu"]?["year"];
                         
                         if (month.HasValue && day.HasValue && year.HasValue)
                         {
                             euReleaseDate = $"{month.Value}/{day.Value}/{year.Value}";
                         }
                     }

                     // Handle JP release date
                     if (amiibo["release"]?["jp"] != null)
                     {
                         int? month = (int?)amiibo["release"]?["jp"]?["month"];
                         int? day = (int?)amiibo["release"]?["jp"]?["day"];
                         int? year = (int?)amiibo["release"]?["jp"]?["year"];
                         
                         if (month.HasValue && day.HasValue && year.HasValue)
                         {
                             jpReleaseDate = $"{month.Value}/{day.Value}/{year.Value}";
                         }
                     }

                     var embed = new EmbedBuilder().WithAuthor(command.User.Username, command.User.GetAvatarUrl())
                         .WithTitle($"Amiibo: {name}")
                         .WithDescription($"Series: {series}")
                         .AddField("Release Dates", $"🇺🇸 US: {usReleaseDate}\n🇪🇺 EU: {euReleaseDate}\n🇯🇵 JP: {jpReleaseDate}")
                         .WithColor(Color.Blue)
                         .Build();

                     await command.RespondAsync(embed: embed);
                      
                    } catch(Exception ex){
                      Console.WriteLine($"Error In Amiibo Command: {ex.Message}");
                      await command.RespondAsync("An error occurred while fetching the amiibo.");
                   }
                   return;
               }
          }
        }
  }
