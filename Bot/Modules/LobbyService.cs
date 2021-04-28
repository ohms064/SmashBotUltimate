using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SmashBotUltimate.Bot.Modules.SavedDataServices;
using SmashBotUltimate.Bot.Validators;
using SmashBotUltimate.Controllers;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Modules {

    public interface ILobbyService {
        Task<ICollection<Lobby>> GetArenas (DiscordGuild guild, DiscordChannel channel, DateTimeOffset queryTime);
        Task<Lobby> Pop (ulong guild, ulong channel, ulong user);
        Task AddArena (Lobby data);
    }

    /// <summary>
    /// Controls the arenas created from Smash Bros Ultimate. After certain hours passes the arena automatically shuts down.
    /// </summary>
    public class LobbyService : ILobbyService {
        private const int HourLimit = 3;

        private PlayerContext _context;
        private ISavedData<object, TimerData> _deleteTimerService;

        private LobbyValidator _validator;

        private TimeSpan _arenaTimeSpan;

        public LobbyService (ISavedData<object, TimerData> deleteTimerService,
            PlayerContext context, LobbyValidator validator) {
            _context = context;
            _deleteTimerService = deleteTimerService;
            _arenaTimeSpan = new TimeSpan (HourLimit, 0, 0);
            _validator = validator;
        }

        public async Task<ICollection<Lobby>> GetArenas (DiscordGuild guild, DiscordChannel channel, DateTimeOffset queryTime, bool specialChannel) {
            var lobbies = specialChannel ?
                await LobbyController.GetGlobalLobbies (_context) :
                await LobbyController.GetLobbies (_context, guild, channel);

            var lobbies2Delete = (from l in lobbies where l.RemovalReferenceTime.AddHours (HourLimit) <= queryTime select l).ToArray ();
            if (lobbies2Delete.Length == 0) {
                return lobbies;
            }

            await LobbyController.DeleteLobbies (_context, lobbies2Delete);
            return await LobbyController.GetLobbies (_context, guild, channel);;
        }

        public async Task OnMessage (DiscordClient client, MessageCreateEventArgs args) {
            if (args.Author.IsBot ||
                args.Message.Content.StartsWith ("s!") || args.Message.Content.StartsWith ("!!")) return;
            var authorId = args.Author.Id;

            if (await ValidateArena (authorId, args.Message.Content, args.Message.Timestamp, args.Guild, args.Channel, args.Author)) {
                await args.Channel.SendMessageAsync ("Se agregó la arena!");
            }
        }

        public async Task<bool> ValidateArena (ulong authorId, string text, DateTimeOffset publishTime, DiscordGuild guild, DiscordChannel channel, DiscordUser user) {
            if (HasCompleteArena (authorId, text, publishTime, out Lobby data)) {
                data.GuildId = guild.Id;
                data.ChannelId = channel.Id;
                data.OwnerId = user.Id;
                await AddArena (data);
                return true;
            }
            return false;
        }

        public async Task<Lobby> Pop (ulong guild, ulong channel, ulong user) {
            var lobby = await LobbyController.PopLobby (_context, guild, channel, user);
            if (lobby != null) {
                _deleteTimerService.RemoveData (new { gId = guild, cId = channel, uId = user });
                return lobby;
            }
            return lobby;
        }
        public async Task MakeArenaGlobal (DiscordGuild guild, DiscordChannel channel, DiscordUser user) {
            var lobby = await LobbyController.GetLobby (_context, guild, channel, user);
            if (lobby.Global) return;
            lobby.Global = true;
            await LobbyController.UpdateLobby (_context, lobby);
        }

        public async Task AddArena (Lobby data) {
            var existingLobby = await LobbyController.GetLobby (_context, data.GuildId, data.ChannelId, data.OwnerId);

            if (existingLobby != null) {
                existingLobby.RoomId = data.RoomId;
                existingLobby.Password = data.Password;
                existingLobby.Comment = data.Comment;
                existingLobby.PublishTime = data.PublishTime;
                await LobbyController.UpdateLobby (_context, existingLobby);
            }
            else {
                var key = new { gId = data.GuildId, cId = data.ChannelId, uId = data.OwnerId };
                await LobbyController.CreateLobby (_context, data);
                var timerData = new TimerData { timeSpan = _arenaTimeSpan };
                timerData.callback += async () => await Pop (data.GuildId, data.ChannelId, data.OwnerId);
                _deleteTimerService.SaveData (key, timerData);
            }
        }

        /// <summary>
        /// Resets the timer for the provided Lobby.
        /// </summary>
        /// <param name="resetTime">The new time that will be used for reference.</param>
        /// <returns>If any lobby was found that could be reseted.</returns>
        public async Task<bool> ResetTimer (DiscordGuild guild, DiscordChannel channel, DiscordUser user, DateTimeOffset resetTime) {
            var lobby = await LobbyController.GetLobby (_context, guild, channel, user);
            if (lobby == null) return false;
            lobby.RemovalReferenceTime = resetTime;
            return true;
        }

        /// <summary>
        /// Checks if the received text matches with the Regex.
        /// <returns>If the arena is valid.</returns>
        private bool HasCompleteArena (ulong authorId, string text, DateTimeOffset publishTime, out Lobby data) {
            if (_validator.IsComplete (text, out data)) {
                data.OwnerId = authorId;
                data.PublishTime = publishTime;
                return true;
            }
            return false;
        }

        //TODO: For arenas with no password?
        private bool HasArenaId (ulong authorId, string text, out Lobby data) {

            if (_validator.IsLobby (text, out data)) {
                data.OwnerId = authorId;
                return true;
            }
            return false;
        }
    }
}