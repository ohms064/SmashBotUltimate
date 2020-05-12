#define CONFIG_FILE
using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Commands;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Bot {
    public class SmashBot {
        public DiscordClient Client { get; private set; }

        public CommandsNextExtension Commands { get; private set; }

        public PlayerContext DBContext { get; private set; }

        public SmashBot (IServiceProvider services) {
            const string path = "config.json";
            string token = "";
#if CONFIG_FILE
            if (!File.Exists (path)) {
                new BotConfig ().Save (path);

                return;
            }
            else {
                token = BotConfig.FromFile (path).Token;
            }
#else
            token = Environment.GetEnvironmentVariable ("smashbot_token", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty (token)) {
                Environment.GetEnvironmentVariable ("smashbot_token", EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty (token)) {
                    Console.WriteLine ("El token smashbot_token debe estar registrado en las variables de entorno! El bot no se inició");
                    return;
                }
            }
#endif

            var config = new DiscordConfiguration {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true,
            };

            Client = new DiscordClient (config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity (new InteractivityConfiguration {
                Timeout = TimeSpan.FromSeconds (20)
            });

            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new string[] { "!", "!!" },
                EnableDms = false,
                EnableMentionPrefix = true,
                Services = services
            };

            var commands = Client.UseCommandsNext (commandsConfig);

            commands.RegisterCommands<ReportCommands> ();
            commands.RegisterCommands<InfoCommands> ();
            commands.RegisterCommands<UtilsCommands> ();
            commands.RegisterCommands<SmashfestCommands> ();
            Client.ConnectAsync ();
        }

        private Task OnClientReady (ReadyEventArgs e) {
            return Task.CompletedTask;
        }
    }
}