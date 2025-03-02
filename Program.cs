using System.Runtime.InteropServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Discord_Bot_Dusk;

public class Program
{
    private static DiscordSocketClient _client;
    public static readonly string[] _timeZones = { "EST", "PST","CT","MT"};
    
    public static async Task Main()
    {
      _client = new DiscordSocketClient();
      _client.Log += Log;
      _client.Ready += Client_Ready;
      _client.GuildAvailable += OnGuildAvailable;
      _client.SlashCommandExecuted += SlashCommandHandler;
      var token = Environment.GetEnvironmentVariable("BotToken");
      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();
      await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    // Create Commands Here
    public static async Task Client_Ready()
    {
        await _client.SetCustomStatusAsync("Checking the Time :3...");
        await _client.SetActivityAsync(new Game($"{DateTime.UtcNow} in UTC", ActivityType.Listening));
        foreach (var guild in _client.Guilds)
        {
            await RegisterCommandsForGuild(guild);
        }
    }
    
    
    /// Add Command Functionality Here
    ///
    /// <param name="command">This Command is Being Checked</param>
    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "current-time":
                var option = command.Data.Options.FirstOrDefault(o => o.Name == "timezones");

                if (option == null || option.Value == null)
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

                await command.RespondAsync($"It is {formattedTime} in {command.User.Mention} TimeZone (sent as {userTimeZone} btw)"); // Only returns the time
                return;
            case "make-sandwich":
                string[] response = { "Tuna", "Chicken", "Turkey", "Beef" };
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
                    await command.RespondAsync($"{DateTime.Today:dddd, MMMM dd yyyy} is the current day.");
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

        }
    }

    private static async Task OnGuildAvailable(SocketGuild guild)
    {
        Console.WriteLine($"Bot joined a new guild: {guild.Name} (ID: {guild.Id})");
        await RegisterCommandsForGuild(guild);
    }
    private static async Task RegisterCommandsForGuild(SocketGuild guild)
    {
        var guildCommands = new[]
        {
            new SlashCommandBuilder()
            .WithName("current-time")
            .WithDescription("Checks for current time in your time zone")
            .AddOption("timezones", ApplicationCommandOptionType.String, "What time zone", true),
            new SlashCommandBuilder()
            .WithName("make-sandwich")
            .WithDescription("Make sandwich for a gluttonous fella"),
            new SlashCommandBuilder()
            .WithName("current-day")
            .WithDescription("Checks for current day in your time zone")
            .AddOption("timezones", ApplicationCommandOptionType.String, "What time zone", false)
        };
        try
        {
            foreach (var command in guildCommands)
            {
                await guild.CreateApplicationCommandAsync(command.Build());
            }
            Console.WriteLine($"Registered commands for {guild.Name} (ID: {guild.Id})");
        }
        catch (ApplicationCommandException ex)
        {
            var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}