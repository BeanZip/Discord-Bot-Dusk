using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Bot_Dusk
{
    public class Program
    {
        private DiscordSocketClient? _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // Use only intents your bot needs and is authorized for
                GatewayIntents = GatewayIntents.Guilds | 
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildMembers |
                                 GatewayIntents.MessageContent // You'll need to enable this in Discord Developer Portal
            });

            // Initialize the command handler
            CommandHandler.Initialize(_client);

            // Setup event handlers CORRECTLY
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            
            // Fix for "GuildAvailable Handler is Blocking the Gateway Task"
            _client.GuildAvailable += guild => 
            {
                // Fire and forget - properly handling exceptions
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await CommandRegistration.RegisterCommandsForGuild(guild);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error registering commands for guild {guild.Name}: {ex.Message}");
                    }
                });
                
                return Task.CompletedTask;
            };
            
            _client.SlashCommandExecuted += CommandHandler.HandleCommand;

            // Get the bot token from environment variables
            string token = Environment.GetEnvironmentVariable("BotToken") 
                ?? throw new InvalidOperationException("Bot token not found in environment variables.");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            if(_client == null) return Task.CompletedTask; // Safety check
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }
    }
}
