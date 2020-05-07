using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
namespace SmashBotUltimate.Bot.Extensions {
    public class DiscordInteractivityPredicateBuilder {
        private List<DiscordMessageReplyFunction> _predicates;
        private readonly CommandContext _context;

        public DiscordInteractivityPredicateBuilder (CommandContext context) {
            _context = context;
            _predicates = new List<DiscordMessageReplyFunction> ();
        }

        public DiscordInteractivityPredicateBuilder SameUser () {
            return ToUser (_context.User);
        }

        public DiscordInteractivityPredicateBuilder InSameChannel () {
            return ToChannel (_context.Channel);
        }

        public DiscordInteractivityPredicateBuilder With (DiscordMessageReplyFunction function) {
            _predicates.Add (function);
            return this;
        }

        public DiscordInteractivityPredicateBuilder ToUser (DiscordUser user) {
            _predicates.Add ((reactionArgs) => {
                return reactionArgs.Author == user;
            });
            return this;
        }

        public DiscordInteractivityPredicateBuilder ToChannel (DiscordChannel channel) {
            _predicates.Add ((reactionArgs) => {
                return reactionArgs.Channel == channel;
            });
            return this;
        }
        public DiscordInteractivityPredicate Build () {
            return new DiscordInteractivityPredicate (_predicates ?? new List<DiscordMessageReplyFunction> ());
        }

        public static implicit operator DiscordInteractivityPredicate (DiscordInteractivityPredicateBuilder builder) {
            return builder.Build ();
        }

        public static implicit operator System.Func<DSharpPlus.Entities.DiscordMessage, bool> (DiscordInteractivityPredicateBuilder builder) {
            return (System.Func<DSharpPlus.Entities.DiscordMessage, bool>) builder.Build ();
        }
    }
}