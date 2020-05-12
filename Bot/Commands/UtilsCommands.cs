using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Bot.Modules.InstructionService;
using SmashBotUltimate.Bot.Modules.DBContextService;

namespace SmashBotUltimate.Bot.Commands {
    public class UtilsCommands : BaseCommandModule {

        public IInteractionService<CoinTossResult, string> CointToss { get; set; }

        public PlayerDBService DBConection { get; set; }

        [Command ("flip")]
        [Aliases ("toss")]
        public async Task FlipCoin (CommandContext context) {
            await CointToss.SimpleAction (context);
        }

        [Command ("init")]
        [RequireOwner]
        public async Task Init (CommandContext context) {
            DBConection.AddGuild(context.Guild.Id, context.Guild.Name);
        }
    }
}