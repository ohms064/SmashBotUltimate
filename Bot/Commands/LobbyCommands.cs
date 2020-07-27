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
            await Lobby.AddArena (data, context.Guild, context.Channel, context.User);
            await context.RespondAsync ("Se registró la sala!");
        }

        [Command ("buscar")]
        [Aliases ("find", "encontrar")]
        private async Task FindArena (CommandContext context) {
            var arenas = await Lobby.GetArenas (context.Guild, context.Channel);
            if (arenas.Count == 0) {
                await context.RespondAsync ("No hay arenas registradas. ¡Publica una!");
                return;
            }
            StringBuilder nameBuilder = new StringBuilder ();
            StringBuilder idBuilder = new StringBuilder ();
            StringBuilder passBuilder = new StringBuilder ();
            StringBuilder timeBuilder = new StringBuilder ();

            StringBuilder builder = new StringBuilder ();
            //builder.AppendLine ("Arenas registradas:");

            var embed = new DiscordEmbedBuilder ().WithTitle ("Arenas registradas");
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
                //embed.AddField("Nombre", owner.DisplayName, true);
                //embed.AddField (owner.DisplayName, $"Id: {arena.roomId.ToUpper()}, Pass: {arena.password, -9}, Tiempo: {durationBuilder.ToString()}");
                //builder.AppendLine ($"{owner.DisplayName:0,10}: Id: {arena.roomId.ToUpper()}, Pass: {arena.password, -9}, Tiempo: {durationBuilder.ToString()}");
                nameBuilder.AppendLine (owner.DisplayName);
                idBuilder.AppendLine (arena.RoomId.ToUpper ());
                passBuilder.AppendLine (arena.Password);
                timeBuilder.AppendLine (durationBuilder.ToString ());
            }

            embed.WithDescription (builder.ToString ());
            embed.AddField ("Nombre", nameBuilder.ToString (), true);
            embed.AddField ("Id", idBuilder.ToString (), true);
            embed.AddField ("Pass", passBuilder.ToString (), true);
            embed.AddField ("Tiempo", timeBuilder.ToString (), true);

            await context.RespondAsync ("", false, embed);
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