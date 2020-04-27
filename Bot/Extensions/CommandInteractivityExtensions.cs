using DSharpPlus.CommandsNext;
using DiscordMessageReplyFunction = System.Func<DSharpPlus.Entities.DiscordMessage, bool>;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace SmashBotUltimate.Bot.Extensions {
    public static class CommandInteractivityExtensions {
        public static DiscordMessageReplyFunction SameChannelResponse (this CommandContext context, DiscordMessageReplyFunction predicate) {
            return (reactionArgs) => {
                return reactionArgs.Channel == context.Channel && predicate.Invoke (reactionArgs);
            };
        }

        /// <summary>
        /// Replies to the same channel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<DiscordMessage> ReplyAsync (this CommandContext context, string message) {
            return await context.Channel.SendMessageAsync (message);
        }
    }
}