using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Modules;

namespace SmashBotUltimate.Bot.Commands {
    [Group ("arena")]
    public class LobbyCommands : BaseCommandModule {

        public ILobbyService Lobby { get; set; }

        [Command ("buscar")]
        [Aliases ("find", "encontrar")]
        private async Task FindArena (CommandContext context) {
            var arenas = Lobby.GetArenas ();
            if (arenas.Count == 0) {
                await context.RespondAsync ("No hay arenas registradas. ¡Publica una!");
                return;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder ();
            builder.AppendLine ("Arenas registradas:");
            foreach (var arena in arenas) {
                var owner = await context.Guild.GetMemberAsync (arena.ownerId);
                builder.AppendLine ($"{owner.DisplayName}: Id: {arena.roomId}, Pass: {arena.password}");;
            }
            await context.RespondAsync (builder.ToString ());
        }

        [Command ("force-cerrar")]
        private async Task CloseArena (CommandContext context, DiscordMember closingMember) {
            var arena = Lobby.Pop (closingMember.Id);
            if (arena != null) {
                await context.RespondAsync ($"Se borró la arena de {closingMember.Mention}!");
            }

        }

        [Command ("cerrar")]
        [Aliases ("close", "terminar")]
        private async Task CloseArena (CommandContext context) {
            await CloseArena (context, context.Member);
        }

    }
}