using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Bot.Modules.InstructionService;

namespace SmashBotUltimate.Bot.Commands {
    [Group ("debug")]
    public class DebugCommands : BaseCommandModule {
        public PlayerDBService DBContext { get; set; }

        [Command ("guilds")]
        public async Task ShowGuilds (CommandContext context) {
            var guilds = await DBContext.GetAllGuilds ();
            foreach (var g in guilds) {
                await context.RespondAsync ($"{g.Name}");
            }
        }
    }
}