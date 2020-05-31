using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SmashBotUltimate.Controllers;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Modules.DBContextService {
    public class PlayerDBService {

        private readonly PlayerContext _context;
        private readonly IMatchmakingFilter _matchFilter;

        public PlayerDBService (PlayerContext context) {
            _context = context;
            _context.Database.Migrate ();
        }

        public async Task<Player> FindPlayer (ulong id) {
            return await PlayerController.GetPlayerWithId (id, _context);
        }

        public async Task<Player> GetPlayer (DiscordMember member,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false) {
            var player = await PlayerController.GetPlayerWithId (member.Id, _context, includeMatches, includeOpponentMatches, includeNicknames, true, false);
            if (player == null) {
                return await CreatePlayer (member);
            } else if (!player.HasGuildId (member.Guild.Id)) {
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

            var first = await GetPlayer (firstPlayerId, includeMatches : true, includeOpponentMatches : true);
            var second = await GetPlayer (secondPlayerId, includeMatches : true, includeOpponentMatches : true);

            var match = await MatchController.GetCompletePlayerMatch (_context, first, second, currentEvent, firstPlayerId.Guild.Id);

            await MatchController.StartPlayerMatch (_context, match[0], match[1]);
            return true;
        }

        public async Task<Match[]> GetMatches (ulong guildId, DiscordMember challenger, DiscordMember opponent) {
            var currentEvent = await GuildController.GetGuildEvent (_context, guildId);

            var challengerMatch = await GetPlayer (challenger, includeMatches : true, includeOpponentMatches : true);
            var opponentMatch = await GetPlayer (opponent, includeMatches : true, includeOpponentMatches : true);

            return await MatchController.GetCompletePlayerMatch (_context, challengerMatch, opponentMatch, currentEvent, challenger.Guild.Id);

        }

        public async Task<bool> SetMatch (ulong guildId, DiscordMember winnerMember, DiscordMember loserMember) {
            var match = await GetMatches (guildId, winnerMember, loserMember);

            var winnerMatch = match[0];
            var loserMatch = match[1];

            if (!winnerMatch.PendingFight || !loserMatch.PendingFight) {
                return false;
            }

            await MatchController.CompletePlayerMatch (_context, winnerMatch, loserMatch);

            return true;
        }

        public async Task<ICollection<Player>> SearchMatch (DiscordMember member, bool onlyRegistered = false) {
            List<Player> matchmakingOpponents = new List<Player> ();
            var player = await PlayerController.GetPlayerWithId (member.Id, _context, includeMatches : true, readOnly : true);
            var pendingPlayers = (from m in player.PlayerMatches where m.PendingFight orderby m.LastMatch select m.OpponentPlayer);
            var pendingMembers = PlayerToMember (member.Guild, pendingPlayers);

            foreach (var p in pendingPlayers) {
                if (await _matchFilter.MatchmakingFilterMember (member.Guild, player, p)) {
                    matchmakingOpponents.Add (p);
                }
            }

            if (onlyRegistered) {
                return matchmakingOpponents;
            }

            var except = (from m in player.PlayerMatches select m.OpponentPlayerId);
            except = except.Append (player.PlayerId);

            var players = await GuildController.GetPlayersInGuild (_context, member.Guild.Id);
            var remaining = from p in players where!except.Contains (p.PlayerId) select p;

            int insertIndex = 0;
            foreach (var r in remaining) {
                if (await _matchFilter.MatchmakingFilterMember (member.Guild, player, r)) {
                    matchmakingOpponents.Insert (insertIndex, r);
                    insertIndex++;
                }
            }

            return matchmakingOpponents;
        }

        public async Task<Guild> ResetGuildCurrentMatch (ulong guildId) {
            return await UpdateGuildCurrentMatch (guildId, Match.DefaultTopic);
        }

        public async Task<Guild> UpdateGuildCurrentMatch (ulong guildId, string match) {
            return await GuildController.UpdateGuildMatch (_context, guildId, match);
        }

        public async Task<string> GetGuildCurrentMatch (ulong guildId) {
            return await GuildController.GetGuildEvent (_context, guildId);
        }

        public async Task AddGuild (ulong guildId, string guildName) {
            var guild = GuildController.GetGuildWithId (_context, guildId);
            if (guild == null) await GuildController.AddGuild (_context, guildId, guildName);
        }

        public async Task<Guild[]> GetAllGuilds () {
            return await GuildController.GetAllGuilds (_context, true);
        }

        private async Task<ICollection<DiscordMember>> PlayerToMember (DiscordGuild guild, IEnumerable<Player> players) {
            var members = new List<DiscordMember> ();
            foreach (var player in players) {
                members.Add (await guild.GetMemberAsync (player.PlayerId));
            }
            return members;
        }
    }
}