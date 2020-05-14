using System.Collections.Generic;
using System.Linq;
using DSharpPlus.CommandsNext;
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

        public Player GetPlayer (DiscordMember member) {
            var player = PlayerController.GetPlayerWithId (member.Id, _context, includeGuildPlayer : true);
            if (player == null) {
                return CreatePlayer (member);
            }
            else if (!player.HasGuildId (member.Guild.Id)) {
                var guild = GuildController.GetGuildWithId (_context, member.Guild.Id);
                PlayerController.AddGuild (_context, player, guild);
            }
            return player;
        }

        public Player CreatePlayer (DiscordMember member) {
            var guild = GuildController.GetGuildWithId (_context, member.Guild.Id);

            //No user in database
            Player player = new Player {
                Name = member.DisplayName,
                PlayerId = member.Id,
                Nivel = 0,

            };
            player.GuildPlayers = new List<GuildPlayer> () {
                new GuildPlayer { Guild = guild, GuildId = member.Guild.Id, Player = player, PlayerId = player.PlayerId }
            };

            PlayerController.AddPlayer (_context, player);

            return player;

        }

        public bool StartMatch (ulong guildId, DiscordMember firstPlayerId, DiscordMember secondPlayerId) {
            var currentEvent = GuildController.GetGuildEvent (_context, guildId);
            if (string.IsNullOrEmpty (currentEvent)) {
                return false;
            }

            currentEvent = $"{guildId}_{currentEvent}";

            var first = GetPlayer (firstPlayerId);
            var second = GetPlayer (secondPlayerId);

            var match = MatchController.GetCompletePlayerMatch (_context, ref first, ref second, currentEvent, true);

            MatchController.StartPlayerMatch (_context, ref match[0], ref match[1]);
            return true;
        }

        public bool SetMatch (ulong guildId, DiscordMember winnerMember, DiscordMember loserMember) {
            var currentEvent = GuildController.GetGuildEvent (_context, guildId);
            if (string.IsNullOrEmpty (currentEvent)) {
                return false;
            }

            currentEvent = $"{guildId}_{currentEvent}";

            var winner = GetPlayer (winnerMember);
            var loser = GetPlayer (loserMember);

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

        public ICollection<Player> SearchMatch (DiscordMember member, bool onlyRegistered = false) {
            List<Player> matchmakingOpponents = new List<Player> ();
            var player = PlayerController.GetPlayerWithId (member.Id, _context, includeMatches : true, readOnly : true);
            var pendingPlayers = (from m in player.PlayerMatches where m.PendingFight select m.OpponentPlayer);

            matchmakingOpponents.AddRange (pendingPlayers);
            if (onlyRegistered) {
                return matchmakingOpponents;
            }

            var except = (from m in player.PlayerMatches select m.OpponentPlayerId);
            except = except.Append (player.PlayerId);

            var players = GuildController.GetPlayersInGuild (_context, member.Guild.Id);
            var remaining = from p in players where!except.Contains (p.PlayerId) select p;

            matchmakingOpponents.AddRange (remaining);

            return matchmakingOpponents;
        }

        public void AddGuild (ulong guildId, string guildName) {
            var guild = GuildController.GetGuildWithId (_context, guildId);
            if (guild == null) GuildController.AddGuild (_context, guildId, guildName);
        }
    }
}