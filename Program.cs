using System.Runtime.InteropServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Discord_Bot_Dusk;

public class Program
{
    private static DiscordSocketClient? _client;

    public static async Task Main()
    {
        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.GuildAvailable += OnGuildAvailable;
        _client.SlashCommandExecuted += CommandHandler.HandleCommand;
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
        if(_client == null)
        {
            return;
        }
        await _client.SetCustomStatusAsync("Checking the Time :3...");
        await _client.SetActivityAsync(new Game($"{DateTime.UtcNow} in UTC", ActivityType.Listening));
        foreach (var guild in _client.Guilds)
        {
            await CommandRegistration.RegisterCommandsForGuild(guild);
        }
    }

    private static async Task OnGuildAvailable(SocketGuild guild)
    {
        Console.WriteLine($"Bot joined a new guild: {guild.Name} (ID: {guild.Id})");
        Console.WriteLine($"Guild has {guild.MemberCount} members");
        await CommandRegistration.RegisterCommandsForGuild(guild);
    }
}