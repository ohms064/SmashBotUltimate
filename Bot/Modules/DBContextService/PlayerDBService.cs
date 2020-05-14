using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using SmashBotUltimate.Controllers;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Modules.DBContextService {
    public class PlayerDBService {
        private readonly PlayerContext _context;
        public PlayerDBService (PlayerContext context) {
            _context = context;
        }

        public async Task<Player> FindPlayer (ulong id) {
            return await PlayerController.GetPlayerWithId (id, _context);
        }

        public async Task<Player> GetPlayer (DiscordMember member) {
            var player = await PlayerController.GetPlayerWithId (member.Id, _context, includeGuildPlayer : true);
            if (player == null) {
                return await CreatePlayer (member);
            }
            else if (!player.HasGuildId (member.Guild.Id)) {
                var guild = await GuildController.GetGuildWithId (_context, member.Guild.Id);
                await PlayerController.AddGuild (_context, player, guild);
            }
            return player;
        }

        public async Task<Player> CreatePlayer (DiscordMember member) {
            var guild = await GuildController.GetGuildWithId (_context, member.Guild.Id);

            //No user in database
            Player player = new Player {
                Name = member.DisplayName,
                PlayerId = member.Id,
                Nivel = 0,

            };
            player.GuildPlayers = new List<GuildPlayer> () {
                new GuildPlayer { Guild = guild, GuildId = member.Guild.Id, Player = player, PlayerId = player.PlayerId }
            };

            await PlayerController.AddPlayer (_context, player);

            return player;

        }

        public async Task<bool> StartMatch (ulong guildId, DiscordMember firstPlayerId, DiscordMember secondPlayerId) {
            var currentEvent = await GuildController.GetGuildEvent (_context, guildId);
            if (string.IsNullOrEmpty (currentEvent)) {
                return false;
            }

            currentEvent = $"{guildId}_{currentEvent}";

            var first = await GetPlayer (firstPlayerId);
            var second = await GetPlayer (secondPlayerId);

            var match = await MatchController.GetCompletePlayerMatch (_context, first, second, currentEvent, true);

            await MatchController.StartPlayerMatch (_context, match[0], match[1]);
            return true;
        }

        public async Task<bool> SetMatch (ulong guildId, DiscordMember winnerMember, DiscordMember loserMember) {
            var currentEvent = await GuildController.GetGuildEvent (_context, guildId);
            if (string.IsNullOrEmpty (currentEvent)) {
                return false;
            }

            currentEvent = $"{guildId}_{currentEvent}";

            var winner = await GetPlayer (winnerMember);
            var loser = await GetPlayer (loserMember);

            if (winner == null || loser == null) {
                return false;
            }

            var match = await MatchController.GetCompletePlayerMatch (_context, winner, loser, currentEvent);

            var winnerMatch = match[0];
            var loserMatch = match[1];

            if (winnerMatch == null || loserMatch == null || !winnerMatch.PendingFight || !loserMatch.PendingFight) {
                return false;
            }

            await MatchController.CompletePlayerMatch (_context, winnerMatch, loserMatch);

            return true;
        }

        public async Task<ICollection<Player>> SearchMatch (DiscordMember member, bool onlyRegistered = false) {
            List<Player> matchmakingOpponents = new List<Player> ();
            var player = await PlayerController.GetPlayerWithId (member.Id, _context, includeMatches : true, readOnly : true);
            var pendingPlayers = (from m in player.PlayerMatches where m.PendingFight orderby m.LastMatch select m.OpponentPlayer);

            matchmakingOpponents.AddRange (pendingPlayers);

            if (onlyRegistered) {
                return matchmakingOpponents;
            }

            var except = (from m in player.PlayerMatches select m.OpponentPlayerId);
            except = except.Append (player.PlayerId);

            var players = await GuildController.GetPlayersInGuild (_context, member.Guild.Id);
            var remaining = from p in players where!except.Contains (p.PlayerId) select p;

            matchmakingOpponents.InsertRange (0, remaining);

            return matchmakingOpponents;
        }

        public async Task AddGuild (ulong guildId, string guildName) {
            var guild = GuildController.GetGuildWithId (_context, guildId);
            if (guild == null) await GuildController.AddGuild (_context, guildId, guildName);
        }
    }
}