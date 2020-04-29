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

namespace SmashBotUltimate.Bot.Commands {
    public class UtilsCommands : BaseCommandModule {

        public IInteractionService<CoinTossResult, string> CointToss { get; set; }

        [Command ("flip")]
        [Aliases ("toss")]
        public async Task FlipCoin (CommandContext context) {
            await CointToss.SimpleAction (context);
        }
    }
}