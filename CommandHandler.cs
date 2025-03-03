using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot_Dusk
{
    public class CommandHandler
    {
        private static readonly string[] _timeZones = { "EST", "PST", "CT", "MT" };
        private static readonly ulong[] DeveloperIds;

        static CommandHandler()
        {
            try
            {
                var fatherIdStr = Environment.GetEnvironmentVariable("FatherId");
                DeveloperIds = !string.IsNullOrEmpty(fatherIdStr) 
                    ? fatherIdStr.Split(',').Where(id => !string.IsNullOrEmpty(id)).Select(ulong.Parse).ToArray() 
                    : Array.Empty<ulong>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing DeveloperIds: {ex.Message}");
                DeveloperIds = Array.Empty<ulong>();
            }
        }

        public static async Task HandleCommand(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "current-time":
                    var option = command.Data.Options.FirstOrDefault(o => o.Name == "timezones");

                    if (option?.Value == null)
                    {
                        await command.RespondAsync("Please provide a valid time zone.");
                        return;
                    }

                    string userTimeZone = option.Value.ToString().ToUpper(); // Normalize case

                    if (!_timeZones.Contains(userTimeZone))
                    {
                        await command.RespondAsync("Invalid Time Zone. Please use: US Time Zone");
                        return;
                    }

                    // Get current UTC time and convert to the selected time zone
                    DateTime currentTime = DateTime.UtcNow;
                    string formattedTime = userTimeZone switch
                    {
                        "EST" => currentTime.AddHours(-5).ToString("hh:mm tt"), // Returns only time
                        "PST" => currentTime.AddHours(-8).ToString("hh:mm tt"),
                        "CT" => currentTime.AddHours(-3).ToString("hh:mm tt"),
                        "MT" => currentTime.AddHours(-2).ToString("hh:mm tt"),
                        _ => "Unknown Time Zone"
                    };

                    await command.RespondAsync($"Currently it is {formattedTime} in {userTimeZone} {command.User.Mention}"); // Only returns the time
                    return;
                case "make-sandwich":
                    string[] response = { "Tuna", "Chicken", "Turkey", "Beef", "Ham", "Pastrami", "BLT", "Club", "Grilled Cheese", "PB&J", "Egg Salad", "Roast Beef", "Italian", "Veggie", "Reuben", "French Dip", "Meatball", "Pulled Pork", "Cuban", "Caprese" };
                    Random random = new Random();
                    int randomIndex = random.Next(response.Length);
                    string responseMessage = response[randomIndex];
                    await command.RespondAsync($"{command.User.Mention} Here is your {responseMessage} sandwich.");
                    return;
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
                    TimeZoneInfo timeZone = userTimeZone2 switch
                    {
                        "EST" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
                        "PST" => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
                        "CT" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
                        "MT" => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
                        _ => TimeZoneInfo.Local // Default case (use local timezone)
                    };

                    DateTime userTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

                    // Send response
                    await command.RespondAsync($"{userTime:dddd, MMMM dd yyyy} is the current day in {userTimeZone2}.");
                    return;
                case "hello-son":
                    try
                    {
                        var fatherId = Environment.GetEnvironmentVariable("FatherId");
                        if (fatherId != null && command.User.Id == ulong.Parse(fatherId))
                        {
                            await command.RespondAsync("Hello Father! Thank you for creating me.");
                        }
                        else
                        {
                            await command.RespondAsync("I'm sorry, I can only respond to my father.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in hello-son command: {ex.Message}");
                    }
                    return;
                case "boom":
                    if (DeveloperIds != null && DeveloperIds.Contains(command.User.Id))
                    {
                        await command.RespondAsync("# 💥 KABOOM 💥 #");
                        await command.FollowupAsync($"{command.User.Mention} has blew himself up");
                    }
                    else
                    {
                        await command.RespondAsync("I'm sorry, I can't let you do that father.");
                        await Task.Delay(2000);
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
                        TimeZoneInfo timeZone2 = timeZoneStr switch
                        {
                            "EST" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
                            "PST" => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
                            "CT" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
                            "MT" => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
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
                case "dev-only-command":
                    if (!DeveloperIds.Contains(command.User.Id))
                    {
                        await command.RespondAsync("This command is restricted to developers only.");
                        return;
                    }

                    // Developer-only command logic here
                    await command.RespondAsync("Developer-only command executed successfully.");
                    return;
            }
        }
    }
}