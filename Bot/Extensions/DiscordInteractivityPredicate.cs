using System.Collections.Generic;
namespace SmashBotUltimate.Bot.Extensions {
    public class DiscordInteractivityPredicate {
        private readonly List<DiscordMessageReplyFunction> _predicates;
        public DiscordInteractivityPredicate (List<DiscordMessageReplyFunction> predicates) {
            _predicates = predicates;
        }

        public static implicit operator DiscordMessageReplyFunction (DiscordInteractivityPredicate interactivityPredicates) {
            return (args) => {
                foreach (var p in interactivityPredicates._predicates) {
                    var result = p.Invoke (args);
                    if (!result) {
                        return false;
                    }
                }
                return true;
            };
        }

        public static implicit operator System.Func<DSharpPlus.Entities.DiscordMessage, bool> (DiscordInteractivityPredicate interactivityPredicates) {
            return (args) => {
                foreach (var p in interactivityPredicates._predicates) {
                    var result = p.Invoke (args);
                    if (!result) {
                        return false;
                    }
                }
                return true;
            };
        }
    }
}