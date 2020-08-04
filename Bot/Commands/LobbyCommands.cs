//#define SIMPLE_MESSAGE
//#define SINGLE_EMBED
#define EMBED

using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Bot.Commands {
    [Group ("arena")]
    public class LobbyCommands : BaseCommandModule {

        public ILobbyService Lobby { get; set; }

        [Command ("nueva")]
        [Aliases ("agregar", "add")]
        private async Task CreateArena (CommandContext context, string id, string password) {
            var data = new Lobby { RoomId = id, Password = password, OwnerId = context.Member.Id };
            await Lobby.AddArena (data, context.Guild, context.Channel, context.User, context.Message.Timestamp);
            await context.RespondAsync ("Se registró la sala!");
        }

        [Command ("buscar")]
        [Aliases ("find", "encontrar")]
        private async Task FindArena (CommandContext context) {
            var specialArena = context.Channel.IsSpecialChannel ();
            var arenas = await Lobby.GetArenas (context.Guild, context.Channel, context.Message.Timestamp, specialArena);
            if (arenas.Count == 0) {
                await context.RespondAsync ("No hay arenas registradas. ¡Publica una!");
                return;
            }

#if SIMPLE_MESSAGE
            //For simple message
            StringBuilder builder = new StringBuilder ();
            builder.AppendLine ("Arenas registradas:");
#endif
#if EMBED
            await context.RespondAsync ("Arenas registradas");
#endif
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

#if SIMPLE_MESSAGE
                builder.AppendLine ($"{owner.DisplayName,-10} Id: {arena.RoomId.ToUpper()}\tPass: {arena.Password, -9}\tTiempo: {durationBuilder.ToString()}");
#endif
#if EMBED

                var title = specialArena ? $"{owner.DisplayName} from {context.Guild.Name}" : owner.DisplayName;
                var userEmnbed = new DiscordEmbedBuilder ().WithTitle (title);
                userEmnbed.AddField ("Id", arena.RoomId.ToUpper (), true);
                userEmnbed.AddField ("Pass", arena.Password, true);
                userEmnbed.AddField ("Tiempo", durationBuilder.ToString (), true);
                await context.RespondAsync ("", false, userEmnbed);
#endif
            }

#if SIMPLE_MESSAGE
            await context.RespondAsync (builder.ToString ());
#endif

        }

        [Command ("force-cerrar")]
        private async Task CloseArena (CommandContext context, DiscordMember closingMember) {
            var arena = await Lobby.Pop (context.Guild, context.Channel, context.User);
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