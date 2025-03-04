using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot_Dusk
{
    public class CommandHandler
    {
        private static readonly string[] _timeZones = { "EST", "PST", "CT", "MT" };
        private static readonly string? devId = Environment.GetEnvironmentVariable("FatherId");
        private static DiscordSocketClient? _client;

        public static void Initialize(DiscordSocketClient client)
        {
            _client = client;
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

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    string userTimeZone = option.Value.ToString().ToUpper(); // Normalize case
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
                        var optionType = command.Data.Options.FirstOrDefault(o => o.Name == "type");
                        Dictionary<int, string> sandwiches = new()
                        {
                            {0, "Tuna"}, {1, "Chicken"}, {2, "Turkey"}, {3, "Beef"}, 
                            {4, "Ham"}, {5, "Pastrami"}, {6, "BLT"}, {7, "Club"}, 
                            {8, "Grilled Cheese"}, {9, "PB&J"}, {10, "Egg Salad"}, 
                            {11, "Roast Beef"}, {12, "Italian"}, {13, "Veggie"}, 
                            {14, "Reuben"}, {15, "French Dip"}, {16, "Meatball"}, 
                            {17, "Pulled Pork"}, {18, "Cuban"}, {19, "Caprese"}
                        };

                        string? optionValue = optionType?.Value.ToString();
                        switch (optionValue)
                        {
                            case "make":
                                Random random = new Random();
                                int randomIndex = random.Next(sandwiches.Count);
                                string responseMessage = sandwiches[randomIndex];
                                await command.RespondAsync($"{command.User.Mention} Here is your {responseMessage} sandwich.");
                                break;

                            case "add":
                                var optionSandwich = command.Data.Options.FirstOrDefault(o => o.Name == "sandwich");
                                if (optionSandwich?.Value == null)
                                {
                                    await command.RespondAsync("Please provide a valid sandwich type to add.");
                                    return;
                                }

                                string? sandwichType = optionSandwich.Value.ToString();
                                if (string.IsNullOrEmpty(sandwichType))
                                {
                                    await command.RespondAsync("Please provide a valid sandwich type to add.");
                                    return;
                                }

                                sandwiches.Add(sandwiches.Count, sandwichType);
                                await command.RespondAsync($"{command.User.Mention} {sandwichType} sandwich has been added to the index.");
                                break;

                            case "menu":
                                string showResponseMessage = "Here are the available sandwiches: ";
                                foreach (var sandwich in sandwiches)
                                {
                                    showResponseMessage += $"{sandwich.Value}, ";
                                }
                                await command.RespondAsync(showResponseMessage);
                                break;

                            default:
                                await command.RespondAsync("Unknown sandwich command option.");
                                break;
                        }
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
                        if (devId != null && ulong.Parse(devId) == command.User.Id)
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
                
                case "delete-command":
                    if (devId != null && command.User.Id == ulong.Parse(devId) || command.User.GlobalName == "Ichiban")
                    {
                        var optionCommand = command.Data.Options.FirstOrDefault(o => o.Name == "command");
                        if (optionCommand?.Value == null)
                        {
                            await command.RespondAsync("Please provide a valid command to delete.");
                            return;
                        }

                        string? commandName = optionCommand.Value.ToString();
                        if (string.IsNullOrEmpty(commandName))
                        {
                            await command.RespondAsync("Please provide a valid command to delete.");
                            return;
                        }
                        else if (!command.GuildId.HasValue)
                        {
                            await command.RespondAsync("This command can only be used in a server.");
                            return;
                        }
                        if(_client == null){
                            Console.WriteLine("Client not found");
                            return;
                        }

                        var commandToDelete = await _client.GetGlobalApplicationCommandAsync(command.GuildId.Value);
                        if (commandToDelete == null)
                        {
                            await command.RespondAsync("Command not found.");
                            return;
                        }

                        await commandToDelete.DeleteAsync();
                        await command.RespondAsync($"Command {commandName} has been deleted.");
                    }
                    else
                    {
                        await command.RespondAsync("You are not authorized to do that!");
                        if(devId == null)
                        {
                            await command.FollowupAsync("Father ID not found. Please set the Father ID in the environment variables.", ephemeral: true);
                            if(ulong.TryParse(Environment.GetEnvironmentVariable("FatherId"), out ulong parsedId) && parsedId == command.User.Id){
                                await command.FollowupAsync($"Father ID has been set to {parsedId}.", ephemeral: true);
                            }
                        }
                    }
                    return;
        }
    }
  }
}