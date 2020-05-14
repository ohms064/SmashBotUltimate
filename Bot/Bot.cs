#define CONFIG_FILE
using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Commands;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Bot {
    public class SmashBot {
        public DiscordClient Client { get; set; }

        public CommandsNextExtension Commands { get; set; }

        public PlayerContext DBContext { get; set; }

        public PlayerDBService DBService { get; set; }

        public SmashBot (IServiceProvider services) {
            DBService = (PlayerDBService) services.GetService (typeof (PlayerDBService));
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
            Client.GuildCreated += OnGuildEntered;
            Client.GuildDeleted += OnGuildLeave;

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

        private Task OnGuildEntered (GuildCreateEventArgs args) {
            Console.WriteLine ($"SmashBot says hi to guild {args.Guild.Name}!");
            return DBService.AddGuild (args.Guild.Id, args.Guild.Name);;
        }

        private Task OnGuildLeave (GuildDeleteEventArgs args) {
            Console.WriteLine ($"Smashbot says bye bye to {args.Guild.Name}");
            return Task.CompletedTask;
        }
    }
}