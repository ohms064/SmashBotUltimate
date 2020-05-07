using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Controllers;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Bot.Modules.DBContextService {
    public class PlayerDBService {
        private readonly PlayerContext _context;
        public PlayerDBService (PlayerContext context) {
            _context = context;
        }

        public Player FindPlayer (ulong id) {
            return PlayerController.GetPlayerWithId (id, _context);
        }

        public bool CreatePlayer (DiscordMember member) {
            var guild = GuildController.GetGuildWithId (_context, member.Guild.Id);
            if (guild == null) {
                return false;
            }

            var userInDb = PlayerController.GetPlayerWithId (member.Id, _context, includeGuildPlayer : true);
            if (userInDb != null) {
                var hasGuild = userInDb.HasGuildId (member.Guild.Id);
                if (hasGuild) {
                    return false; //Player already created and in guild
                }

                GuildController.AddGuildToPlayer (_context, ref guild, ref userInDb);
                return true;
            }
            //No user in database
            Player player = new Player {
                Name = member.DisplayName,
                PlayerId = member.Id,
                Nivel = 0
            };

            PlayerController.AddPlayer (_context, player);

            return true;
        }

        public bool StartMatch (ulong guildId, ulong firstPlayerId, ulong secondPlayerId) {
            var currentEvent = GuildController.GetGuildEvent (_context, guildId);
            if (string.IsNullOrEmpty (currentEvent)) {
                return false;
            }

            currentEvent = $"{guildId}_{currentEvent}";

            var first = PlayerController.GetPlayerWithId (firstPlayerId, _context, includeMatches : true);
            var second = PlayerController.GetPlayerWithId (secondPlayerId, _context, includeMatches : true);

            if (first == null || second == null) {
                return false;
            }

            var match = MatchController.GetCompletePlayerMatch (_context, ref first, ref second, currentEvent, true);

            MatchController.StartPlayerMatch (_context, ref match[0], ref match[1]);
            return true;
        }

        public bool SetMatch (ulong guildId, ulong winnerId, ulong loserId) {
            var currentEvent = GuildController.GetGuildEvent (_context, guildId);
            if (string.IsNullOrEmpty (currentEvent)) {
                return false;
            }

            currentEvent = $"{guildId}_{currentEvent}";

            var winner = PlayerController.GetPlayerWithId (winnerId, _context);
            var loser = PlayerController.GetPlayerWithId (loserId, _context);

            if (winner == null || loser == null) {
                return false;
            }

            var match = MatchController.GetCompletePlayerMatch (_context, ref winner, ref loser, currentEvent);

            var winnerMatch = match[0];
            var loserMatch = match[1];

            if (winnerMatch == null || loserMatch == null || !winnerMatch.PendingFight || !loserMatch.PendingFight) {
                return false;
            }

            MatchController.CompletePlayerMatch (_context, ref winnerMatch, ref loserMatch);

            return true;

        }
    }
}