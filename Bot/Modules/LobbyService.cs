using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SmashBotUltimate.Bot.Modules.SavedDataServices;
using SmashBotUltimate.Controllers;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Modules {

    public interface ILobbyService {
        Task<ICollection<Lobby>> GetArenas (DiscordGuild guild, DiscordChannel channel, DateTimeOffset queryTime, bool specialChannel);
        Task<Lobby> Pop (DiscordGuild guild, DiscordChannel channel, DiscordUser user);
        Task AddArena (Lobby data, DiscordGuild guild, DiscordChannel channel, DiscordUser user, DateTimeOffset publishTime);

        Task<bool> ResetTimer (DiscordGuild guild, DiscordChannel channel, DiscordUser user, DateTimeOffset resetTime);
    }

    /// <summary>
    /// Controls the arenas created from Smash Bros Ultimate. After certain hours passes the arena automatically shuts down.
    /// </summary>
    public class LobbyService : ILobbyService {
        private const int HourLimit = 4;
        private const string arenaIdPattern = @"(\w{5})";
        private const string arenaPassPattern = @"(\d+)";
        private const string arenaCompletePattern = @"\b(\w{5})\s?(/|-)\s?(\d{1,8})\b";

        private readonly PlayerContext _context;
        private readonly Regex _completeRegex, _passRegex, _idRegex;
        private readonly TimeSpan _arenaTimeSpan;

        public LobbyService (PlayerContext context) {
            _context = context;
            _arenaTimeSpan = TimeSpan.FromHours (HourLimit);

            _passRegex = new Regex (arenaPassPattern);
            _idRegex = new Regex (arenaIdPattern);
            _completeRegex = new Regex (arenaCompletePattern);
        }

        public async Task<ICollection<Lobby>> GetArenas (DiscordGuild guild, DiscordChannel channel, DateTimeOffset queryTime, bool specialChannel) {
            var lobbies = specialChannel ?
                await LobbyController.GetLobbies (_context, channel.Name) :
                await LobbyController.GetLobbies (_context, guild, channel);

            var lobbies2Delete = (from l in lobbies where l.RemovalReferenceTime.AddHours (HourLimit) <= queryTime select l).ToArray ();
            if (lobbies2Delete.Length == 0) {
                return lobbies;
            }

            await LobbyController.DeleteLobbies (_context, lobbies2Delete);
            return await LobbyController.GetLobbies (_context, guild, channel);;
        }

        public async Task OnMessage (MessageCreateEventArgs args) {
            if (args.Author.IsBot ||
                args.Message.Content.StartsWith ("s!") || args.Message.Content.StartsWith ("!!")) return;
            var authorId = args.Author.Id;

            if (await ValidateArena (authorId, args.Message.Content, args.Message.Timestamp, args.Guild, args.Channel, args.Author)) {
                await args.Channel.SendMessageAsync ("Se agregó la arena!");
            }
        }

        public async Task<bool> ValidateArena (ulong authorId, string text, DateTimeOffset publishTime, DiscordGuild guild, DiscordChannel channel, DiscordUser user) {
            if (HasCompleteArena (authorId, text, publishTime, out Lobby data)) {
                await AddArena (data, guild, channel, user, publishTime);
                return true;
            }
            return false;
        }

        public async Task<Lobby> Pop (DiscordGuild guild, DiscordChannel channel, DiscordUser user) {
            var lobby = await LobbyController.PopLobby (_context, guild, channel, user);
            if (lobby != null) {

                return lobby;
            }
            return lobby;
        }

        public async Task AddArena (Lobby data, DiscordGuild guild, DiscordChannel channel, DiscordUser user, DateTimeOffset publishTime) {
            var existingLobby = await LobbyController.GetLobby (_context, guild, channel, user);

            if (existingLobby != null) {
                existingLobby.RoomId = data.RoomId;
                existingLobby.Password = data.Password;
                existingLobby.RemovalReferenceTime = publishTime;
                await LobbyController.UpdateLobby (_context, existingLobby);
            } else {
                var key = new { gId = guild.Id, cId = channel.Id, uId = user.Id };
                await LobbyController.CreateLobby (_context, guild, channel, user, data.RoomId, data.Password, publishTime);
            }
        }

        public async Task<bool> ResetTimer (DiscordGuild guild, DiscordChannel channel, DiscordUser user, DateTimeOffset resetTime) {
            var lobby = await LobbyController.GetLobby (_context, guild, channel, user);
            if (lobby == null) return false;
            lobby.RemovalReferenceTime = resetTime;
            return true;
        }

        private bool HasCompleteArena (ulong authorId, string text, DateTimeOffset publishTime, out Lobby data) {
            var match = _completeRegex.Match (text);
            if (match.Success) {
                data = new Lobby ();
                data.RoomId = match.Groups[1].Value;
                data.Password = match.Groups[3].Value;
                data.OwnerId = authorId;
                data.PublishTime = publishTime;
                return true;
            }
            data = null;
            return false;
        }

        private bool HasArenaId (ulong authorId, string text, out Lobby data) {
            var match = _idRegex.Match (text);
            if (match.Success) {
                data = new Lobby ();
                data.RoomId = match.Groups[1].Value;
                data.OwnerId = authorId;
                return true;
            }
            data = null;
            return false;
        }
    }
}