//define CONFIG_FILE
using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Commands;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Bot {
    public class SmashBot {
        public DiscordClient Client { get; set; }

        public CommandsNextExtension Commands { get; set; }

        public PlayerContext DBContext { get; set; }

        public PlayerDBService DBService { get; set; }

        public LobbyService Lobby;

        public SmashBot (IServiceProvider services) {
            DBService = (PlayerDBService) services.GetService (typeof (PlayerDBService));
            Lobby = (LobbyService) services.GetService (typeof (ILobbyService));
            const string tokenKey = "smashbot_token";
            string token = "";
#if CONFIG_FILE
            const string path = "config.json";
            if (!File.Exists (path)) {
                new BotConfig ().Save (path);

                return;
            } else {
                token = BotConfig.FromFile (path).Token;
            }
#else
            token = Environment.GetEnvironmentVariable (tokenKey);
            if (string.IsNullOrEmpty (token)) {
                Console.WriteLine ($"El token {tokenKey} debe estar registrado en las variables de entorno! El bot no se inició");
                return;
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
            Client.MessageCreated += Lobby.OnMessage;

            Client.UseInteractivity (new InteractivityConfiguration {
                Timeout = TimeSpan.FromSeconds (20)
            });

            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new string[] { "s!", "!!" },
                EnableDms = false,
                EnableMentionPrefix = true,
                Services = services
            };

            var commands = Client.UseCommandsNext (commandsConfig);

            //commands.RegisterCommands<ReportCommands> ();
            //commands.RegisterCommands<InfoCommands> ();
            commands.RegisterCommands<UtilsCommands> ();
            //commands.RegisterCommands<SmashfestCommands> ();
            commands.RegisterCommands<LobbyCommands> ();
            //commands.RegisterCommands<DebugCommands> ();
            commands.RegisterCommands<TimerCommands> ();
            Client.ConnectAsync ();
        }

        private Task OnClientReady (ReadyEventArgs e) {
            return Task.CompletedTask;
        }

        private Task OnMessageCreated (MessageCreateEventArgs e) {
            return Task.CompletedTask;
        }

        private async Task OnGuildEntered (GuildCreateEventArgs args) {
            Console.WriteLine ($"SmashBot says hi to guild {args.Guild.Name}!");
            var channel = args.Guild.SystemChannel;
            await channel.SendMessageAsync ($"Saludos {args.Guild.Name}!");
            await DBService.AddGuild (args.Guild.Id, args.Guild.Name);
        }

        private Task OnGuildLeave (GuildDeleteEventArgs args) {
            Console.WriteLine ($"Smashbot says bye bye to {args.Guild.Name}");
            return Task.CompletedTask;
        }
    }
}