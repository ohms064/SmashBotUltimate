using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using SmashBotUltimate.Bot.Modules.SavedDataServices;
namespace SmashBotUltimate.Bot.Modules {

    public interface ILobbyService {
        List<LobbyData> GetArenas ();
        LobbyData Pop (ulong id);
        void AddArena (LobbyData data);
    }

    public class LobbyService : ILobbyService {
        private ISavedData<string, TimerData> _deleteTimerService;
        private Dictionary<ulong, LobbyData> _arenas;

        private const string arenaIdPattern = @"(\w{5})";
        private const string arenaPassPattern = @"(\d+)";
        private const string arenaCompletePattern = @"\b(\w{5})\s?(/|-)\s?(\d{1,8})\b";
        private Regex _completeRegex, _passRegex, _idRegex;

        private const string listenChannel = "retas";

        private TimeSpan _arenaTimeSpan;

        public LobbyService (ISavedData<string, TimerData> deleteTimerService) {
            _arenas = new Dictionary<ulong, LobbyData> ();
            _deleteTimerService = deleteTimerService;
            _arenaTimeSpan = new TimeSpan (5, 0, 0); //5 horas

            _passRegex = new Regex (arenaPassPattern);
            _idRegex = new Regex (arenaIdPattern);
            _completeRegex = new Regex (arenaCompletePattern);
        }

        public List<LobbyData> GetArenas () {
            return new List<LobbyData> (_arenas.Values);
        }

        public async Task OnMessage (MessageCreateEventArgs args) {
            if (!args.Channel.Name.Equals (listenChannel) || args.Author.IsBot ||
                args.Message.Content.StartsWith ("s!") || args.Message.Content.StartsWith ("!!")) return;
            var authorId = args.Author.Id;

            if (ValidateArena (authorId, args.Message.Content, args.Message.Timestamp)) {
                await args.Channel.SendMessageAsync ("Se agregó la arena!");
            }
            /*
            if (HasArenaId (authorId, args.Message.Content, out LobbyData partialData)) {
                AddArena (authorId, partialData);
                return;
            }
            if (UpdateArenaPassword (authorId, args.Message.Content)) {
                await args.Channel.SendMessageAsync ("Se agregó la sala!");
                return;
            }
            */
        }

        public void AddArena (LobbyData data) {
            if (_arenas.ContainsKey (data.ownerId)) {
                _arenas[data.ownerId] = data;
            } else {
                _arenas.Add (data.ownerId, data);
            }
        }

        public bool ValidateArena (ulong authorId, string text, DateTimeOffset publishTime) {
            if (HasCompleteArena (authorId, text, publishTime, out LobbyData data)) {
                AddArena (authorId, data);
                return true;
            }
            return false;
        }

        public LobbyData Pop (ulong id) {
            if (_arenas.ContainsKey (id)) {
                var lobby = _arenas[id];
                _arenas.Remove (id);
                _deleteTimerService.RemoveData (IdToKey (id));
                return lobby;
            }
            return null;
        }

        private void AddArena (ulong authorId, LobbyData data) {
            if (_arenas.ContainsKey (authorId)) {
                _arenas[authorId] = data;
            } else {
                _arenas.Add (authorId, data);
                var timerData = new TimerData { timeSpan = _arenaTimeSpan };
                timerData.callback += () => Pop (authorId);
                _deleteTimerService.SaveData (IdToKey (authorId), timerData);
            }
        }

        private bool HasCompleteArena (ulong authorId, string text, DateTimeOffset publishTime, out LobbyData data) {
            var match = _completeRegex.Match (text);
            if (match.Success) {
                data = new LobbyData ();
                data.roomId = match.Groups[1].Value;
                data.password = match.Groups[3].Value;
                data.ownerId = authorId;
                data.publishTime = publishTime;
                return true;
            }
            data = null;
            return false;
        }

        private bool HasArenaId (ulong authorId, string text, out LobbyData data) {
            var match = _idRegex.Match (text);
            if (match.Success) {
                data = new LobbyData ();
                data.roomId = match.Groups[1].Value;
                data.ownerId = authorId;
                return true;
            }
            data = null;
            return false;
        }

        private bool UpdateArenaPassword (ulong authorId, string text) {
            if (!_arenas.ContainsKey (authorId)) return false;
            if (string.IsNullOrEmpty (_arenas[authorId].password)) return false;
            var match = _passRegex.Match (text);
            if (match.Success) {
                _arenas[authorId].password = match.Groups[1].Value;
                return true;
            }
            return false;
        }

        private string IdToKey (ulong id) {
            return $"LobbyService_{id}";
        }
    }

    public class LobbyData {
        public string roomId;
        public string password;
        public ulong ownerId;
        public DateTimeOffset publishTime;

        public TimeSpan Duration (DateTimeOffset other) {
            return other - publishTime;
        }

    }
}