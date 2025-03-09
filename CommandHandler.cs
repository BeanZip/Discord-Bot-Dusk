using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                        TimeZones.HST => currentTime.AddHours(-10).ToString("hh:mm tt"),
                        TimeZones.AKST => currentTime.AddHours(-9).ToString("hh:mm tt"),
                        TimeZones.PST => currentTime.AddHours(-8).ToString("hh:mm tt"),
                        TimeZones.MST => currentTime.AddHours(-7).ToString("hh:mm tt"),
                        TimeZones.CST => currentTime.AddHours(-6).ToString("hh:mm tt"),
                        TimeZones.EST => currentTime.AddHours(-5).ToString("hh:mm tt"),
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
                            {20, "Philly Cheesesteak"}
                        };
                                Random random = new Random(); 
                                int randomIndex = random.Next(sandwiches.Count);
                                string responseMessage = sandwiches[randomIndex];
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
                       if(devId != null && ulong.Parse(devId) == command.User.Id)
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
                    
                    } else{
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

                        TimeZoneInfo timeZone2 = parsedTimezone switch
                        {
                            TimeZones.EST => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
                            TimeZones.PST => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
                            TimeZones.CST => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
                            TimeZones.MST => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
                            TimeZones.AKST => TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"),
                            TimeZones.HST => TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"),
                            _ => TimeZoneInfo.Local // Default case (use local timezone)
                        };

                        // Convert the date to the given time zone
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

                        if(devId == null){
                            await command.RespondAsync("Developer ID not found.");
                            return;
                        }
                    
                    if(command.User.Id == ulong.Parse(devId) && !command.User.IsBot)
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
                    } else{
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
                            await Task.Delay(300);
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

                    if(optionOperation?.Value == null || optionNum1?.Value == null || optionNum2?.Value == null)
                    {
                        await command.RespondAsync("Please provide a valid operation and two numbers to perform the operation on.");
                        return;
                    } else{
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
                            catch(DivideByZeroException) { return double.NaN; }
                        }

                        if (double.IsNaN(result))
                        {
                            await command.RespondAsync("Invalid operation or division by zero.");
                        }
                        else
                        {
                            await command.RespondAsync($"Result: {result}");
                        }
                    }
                    return;
        }
    }
  }
}