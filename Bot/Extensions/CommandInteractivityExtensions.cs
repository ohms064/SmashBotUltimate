using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SmashBotUltimate.Bot.Extensions {

    public delegate bool DiscordMessageReplyFunction (DiscordMessage e);
    public static class CommandInteractivityExtensions {

        public static DiscordInteractivityPredicateBuilder WithPredicate (this CommandContext context) {
            return new DiscordInteractivityPredicateBuilder (context);
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