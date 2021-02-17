using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Converters;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Commands {
    [Group ("arena")]
    public class LobbyCommands : BaseCommandModule {

        public ILobbyService Lobby { get; set; }

        public IConvert<Lobby> Converter { get; set; }

        [Command ("nueva")]
        [Aliases ("agregar", "add")]
        private async Task CreateArena (CommandContext context, [RemainingText] string args) {
            if (!Converter.Convert (args, context, out Lobby data)) return;
            await Lobby.AddArena (data);
            await context.RespondAsync ("Se registró la sala!");
        }

        [Command ("Test")]
        private async Task Test (CommandContext context, bool s) {
            await context.RespondAsync ($"Se ingresó {s}");
        }

        [Command ("buscar")]
        [Aliases ("find", "encontrar")]
        private async Task FindArena (CommandContext context) {
            var arenas = await Lobby.GetArenas (context.Guild, context.Channel, context.Message.Timestamp);
            if (arenas.Count == 0) {
                await context.RespondAsync ("No hay arenas registradas. ¡Publica una!");
                return;
            }

            await context.RespondAsync ("Arenas registradas");

            foreach (var arena in arenas) {
                var owner = await context.Guild.GetMemberAsync (arena.OwnerId);
                var duration = arena.Duration (context.Message.Timestamp);
                var durationBuilder = new System.Text.StringBuilder ();
                if (duration.Hours > 0) {
                    durationBuilder.Append ($"{duration.Hours} hora(s) ");
                }
                if (duration.Minutes > 0) {
                    durationBuilder.Append ($"{duration.Minutes} min(s) ");
                }
                if (durationBuilder.Length == 0) {
                    durationBuilder.Append ("¡Recien creada!");
                }

                var userEmnbed = new DiscordEmbedBuilder ().WithTitle (owner.DisplayName);
                userEmnbed.AddField ("Id", arena.RoomId.ToUpper (), true);
                if (arena.HasPassword) userEmnbed.AddField ("Pass", arena.Password, true);
                userEmnbed.AddField ("Tiempo", durationBuilder.ToString (), true);
                if (arena.HasComment) userEmnbed.AddField ("Extras", arena.Comment);
                await context.RespondAsync (userEmnbed);
            }

        }

        [Command ("force-close")]
        [Aliases ("forzar-cierre")]
        private async Task CloseArena (CommandContext context, DiscordMember closingMember) {
            var arena = await Lobby.Pop (context.Guild.Id, context.Channel.Id, context.User.Id);
            if (arena != null) {
                await context.RespondAsync ($"Se borró la arena de { closingMember.Mention }!");
            }
        }

        [Command ("cerrar")]
        [Aliases ("close", "terminar")]
        private async Task CloseArena (CommandContext context) {
            await CloseArena (context, context.Member);
        }
    }
}