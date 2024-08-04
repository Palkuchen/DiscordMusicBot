
//MTE3MjI0NjYzOTYzMzc2MDMxNw.GLh3YH.BWd-6vrptzJHmG12gqI_0VgYcXalZiM8umtfUM
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using MusicCrackBot;
using DSharpPlus.EventArgs;
using MusicCrackBot.commands;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;

namespace MusicCrackBot
{

 public sealed class Program
{
    public static DiscordClient Client { get; set; }
    static async Task Main(string[] args)
    {
        //1. Retrieve Token/Prefix from config.json
        var configProperties = new JSONReader();
        await configProperties.ReadJSON();

        //2. Create the Bot Configuration
        var discordConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            Token = configProperties.discordToken,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
        };

        //3. Initialise the DiscordClient property
        Client = new DiscordClient(discordConfig);

        //Set defaults for interactivity based events
        Client.UseInteractivity(new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromMinutes(2)
        });

        Client.Ready += Client_Ready;

        //5. Create the Command Configuration
        var commandsConfig = new CommandsNextConfiguration
        {
            StringPrefixes = new string[] { configProperties.discordPrefix },
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        };

            //6. Initialize the CommandsNextExtention property
            CommandsNextExtension Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<MusicCMD>();

            //Lavalink Configuration
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "v3.lavalink.rocks",
                Port = 443,
                Secured = true
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "horizxon.tech",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();


            //8. Connect the Client to the Discord Gateway
            await Client.ConnectAsync();

            await lavalink.ConnectAsync(lavalinkConfig);

            //Make sure you delay by -1 to keep the bot running forever
            await Task.Delay(-1);
    }

    private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }
    }
}