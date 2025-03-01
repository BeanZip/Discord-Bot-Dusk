using System.Runtime.InteropServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Discord_Bot_Dusk;

public class Program
{
    private static DiscordSocketClient _client;
    
    public static async Task Main()
    {
      _client = new DiscordSocketClient();
      _client.Log += Log;
      _client.Ready += Client_Ready;
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


    public static async Task Client_Ready()
    {
        ulong guildId = 928791142475104277;
        var guild = _client.GetGuild(guildId);
        
        var guildCommand1 = new Discord.SlashCommandBuilder()
            .WithName("Current-Time")
            .WithDescription("Checks For Current Time in Your Time Zone");

        try
        {
            await guild.CreateApplicationCommandAsync(guildCommand1.Build());
            await _client.SetGameAsync($"Checking the Clock :3");
        }
        catch (ApplicationCommandException ex)
        {
            var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "current-time":
                await command.RespondAsync($"Current time is {DateTime.Now}");
                break;
        }
    }
}