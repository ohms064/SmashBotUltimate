using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Bot.Modules.InstructionService;

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
        [Hidden]
        public async Task Init (CommandContext context) {
            await DBConection.AddGuild (context.Guild.Id, context.Guild.Name);
            await context.RespondAsync ("Added guild to DB");
        }
    }
}