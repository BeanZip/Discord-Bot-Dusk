using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Bot_Dusk
{
    /// <summary>
    /// Register the commands for the bot to use
    /// </summary>
    public class CommandRegistration
    {
        /// <summary>
        /// Register the commands for a specific guild
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
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
                        .WithDescription("Set a reminder for a specific time in a Date formatted as MM/dd/yyyy")
                        .AddOption("date", ApplicationCommandOptionType.String, "The date to set the reminder for", isRequired: true)
                        .AddOption("message", ApplicationCommandOptionType.String, "The message to remind you with", isRequired: true)
                        .AddOption("timezones", ApplicationCommandOptionType.String, "The timezone to check", isRequired: false),

                    new SlashCommandBuilder()
                        .WithName("delete-command")
                        .WithDescription("Delete a command from the bot")
                        .AddOption("command", ApplicationCommandOptionType.String, "The command to delete", isRequired: true),

                    new SlashCommandBuilder()
                        .WithName("roulette") 
                        .WithDescription("Play a game of Russian Roulette")
                        .AddOption("bullets", ApplicationCommandOptionType.Integer, "The number of bullets to load", isRequired: true),
                        
                    new SlashCommandBuilder().WithName("math")
                        .WithDescription("Perform a math operation")
                        .AddOption("operation", ApplicationCommandOptionType.String, "The operation to perform", isRequired: true)
                        .AddOption("num1", ApplicationCommandOptionType.Integer, "The first number", isRequired: true)
                        .AddOption("num2", ApplicationCommandOptionType.Integer, "The second number", isRequired: true),
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