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

                await command.RespondAsync(formattedTime); // Only returns the time
                return;
            case "make-sandwich":
                string[] response = { "Tuna", "Chicken", "Turkey", "Beef" };
                Random random = new Random();
                int randomIndex = random.Next(response.Length);
                string responseMessage = response[randomIndex];
                await command.RespondAsync($"Here is your {responseMessage} sandwich.");
                return;
            case "current-day":
                DateTime currentDay = DateTime.Today;
                await command.RespondAsync($"{currentDay.DayOfWeek} is the current day on {currentDay:MMMM yyyy}.");
                return;
            case "inspiring-quote":
                OpenAIClient aiClient = new OpenAIClient();
                string openAiResponse =await aiClient.GetResponseAsync("Give a inspiring quote");
                await command.RespondAsync(openAiResponse);
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
        var guildCommand1 = new SlashCommandBuilder()
            .WithName("current-time")
            .WithDescription("Checks for current time in your time zone")
            .AddOption("timezones", ApplicationCommandOptionType.String, "What time zone", true);
        var guildCommand2 = new SlashCommandBuilder()
            .WithName("make-sandwich")
            .WithDescription("Make sandwich for a gluttonous fella");
        var guildCommand3 = new SlashCommandBuilder()
            .WithName("current-day")
            .WithDescription("Checks for current day in your time zone");
        var guildCommand4 = new SlashCommandBuilder()
            .WithName("inspiring-quote")
            .WithDescription("Gives you a quote to wake up better rejuvenating tomorrow");
        try
        {
            await guild.CreateApplicationCommandAsync(guildCommand1.Build());
            await guild.CreateApplicationCommandAsync(guildCommand2.Build());
            await guild.CreateApplicationCommandAsync(guildCommand3.Build());
            await guild.CreateApplicationCommandAsync(guildCommand4.Build());
            Console.WriteLine($"Registered commands for {guild.Name} (ID: {guild.Id})");
        }
        catch (ApplicationCommandException ex)
        {
            var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}