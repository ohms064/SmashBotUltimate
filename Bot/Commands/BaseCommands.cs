using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SmashBotUltimate.Bot.Commands {
    public class BaseCommands : BaseCommandModule {
        /// <summary>
        /// Replies to the same channel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> ReplyAsync (CommandContext context, string message) {
            return await context.Channel.SendMessageAsync (message);
        }
    }
}