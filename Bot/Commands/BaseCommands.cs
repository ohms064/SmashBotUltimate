using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SmashBotUltimate.Bot.Commands {
    public class BaseCommands : BaseCommandModule {
        public async Task ReplyAsync (CommandContext context, string message) {
            await context.Channel.SendMessageAsync (message);
        }
    }
}