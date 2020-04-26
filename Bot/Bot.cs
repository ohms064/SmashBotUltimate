using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using SmashBotUltimate.Bot.Commands;

namespace SmashBotUltimate.Bot {
    public class SmashBot {
        public DiscordClient Client { get; private set; }

        public CommandsNextExtension Commands { get; private set; }

        public SmashBot (IServiceProvider services) {
            string token = Environment.GetEnvironmentVariable ("smashbot_token", EnvironmentVariableTarget.Machine) ??
                Environment.GetEnvironmentVariable ("smashbot_token", EnvironmentVariableTarget.User);

            var config = new DiscordConfiguration {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };

            Client = new DiscordClient (config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity (new InteractivityConfiguration {
                Timeout = TimeSpan.FromSeconds (20)
            });

            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new string[] { "!" },
                EnableDms = false,
                EnableMentionPrefix = true,
                Services = services
            };

            var commands = Client.UseCommandsNext (commandsConfig);

            commands.RegisterCommands<ReportCommands> ();
            commands.RegisterCommands<InfoCommands> ();

            Client.ConnectAsync ();
        }

        private Task OnClientReady (ReadyEventArgs e) {
            return Task.CompletedTask;
        }
    }
}