using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Bot_Dusk
{
    public class CommandRegistration
    {
        public static async Task RegisterCommandsForGuild(SocketGuild guild)
        {
            try
            {
                var commands = new SlashCommandBuilder[]
                {
                    new SlashCommandBuilder()
                        .WithName("current-time")
                        .WithDescription("Get the current time in a specific timezone")
                        .AddOption("timezones", ApplicationCommandOptionType.String, "The timezone to check", isRequired: true),

                    new SlashCommandBuilder()
                        .WithName("make-sandwich")
                        .WithDescription("Get a random sandwich suggestion"),

                    new SlashCommandBuilder()
                        .WithName("current-day")
                        .WithDescription("Get the current day")
                        .AddOption("timezones", ApplicationCommandOptionType.String, "The timezone to check", isRequired: false),

                    new SlashCommandBuilder()
                        .WithName("hello-son")
                        .WithDescription("Special greeting for father"),

                    new SlashCommandBuilder()
                        .WithName("boom")
                        .WithDescription("Boom!"),

                    new SlashCommandBuilder()
                        .WithName("set-timer")
                        .WithDescription("Set a timer for a specific time in Seconds")
                        .AddOption("time", ApplicationCommandOptionType.Integer, "The time to set the timer for", isRequired: true),

                    new SlashCommandBuilder()
                        .WithName("set-reminder")
                        .WithDescription("Set a reminder for a specific time in a Date")
                        .AddOption("date", ApplicationCommandOptionType.String, "The date to set the reminder for", isRequired: true)
                        .AddOption("message", ApplicationCommandOptionType.String, "The message to remind you with", isRequired: true)
                        .AddOption("timezones", ApplicationCommandOptionType.String, "The timezone to check", isRequired: false)
                };

                foreach (var cmd in commands)
                {
                    await guild.CreateApplicationCommandAsync(cmd.Build());
                }
            }
            catch (HttpException exception)
            {
                Console.WriteLine($"Error creating commands in guild {guild.Name}: {exception.Message}");
            }
        }
    }
}