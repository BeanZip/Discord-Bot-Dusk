﻿using System.Runtime.InteropServices;
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
        string formattedTime = DateTime.UtcNow.ToString("HH:mm");
        // Set initial status
        await _client.SetCustomStatusAsync("Checking the Time :3...");
        
        // Create a timer that updates the status every minute
        var timer = new System.Timers.Timer(60000); // 60000 ms = 1 minute
        timer.Elapsed += async (sender, e) => {
            try {
            formattedTime = DateTime.UtcNow.ToString("h:mm tt"); // 12-hour format with AM/PM
            await _client.SetActivityAsync(new Game($"It is now {formattedTime} in UTC", ActivityType.Listening));
            } catch (Exception ex) {
            Console.WriteLine($"Error updating status: {ex.Message}");
            }
        };
        timer.AutoReset = true;
        timer.Enabled = true;
        
        // Check if daylight saving time is in effect
        bool isDST = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now);
        string dstStatus = isDST ? "(DST Active)" : "(Standard Time)";
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