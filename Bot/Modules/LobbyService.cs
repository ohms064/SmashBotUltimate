using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
namespace SmashBotUltimate.Bot.Modules {

    public interface ILobbyService {
        List<LobbyData> GetArenas ();
        LobbyData Pop (ulong id);
        bool ValidateArena (ulong authorId, string text);

        void AddArena (LobbyData data);
    }

    public class LobbyService : ILobbyService {
        private Dictionary<ulong, LobbyData> _arenas;
        private Dictionary<ulong, Task> _deleteTasks;

        private const string arenaIdPattern = @"(\w{5})";
        private const string arenaPassPattern = @"(\d+)";
        private const string arenaCompletePattern = @"\b(\w{5})\s?\S\s?(\d+)\b";
        private Regex _completeRegex, _passRegex, _idRegex;

        private const string listenChannel = "retas";

        private TimeSpan _arenaTimeSpan;

        public LobbyService () {
            _arenas = new Dictionary<ulong, LobbyData> ();
            _deleteTasks = new Dictionary<ulong, Task> ();
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
            if (ValidateArena (authorId, args.Message.Content)) {
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

        public bool ValidateArena (ulong authorId, string text) {
            if (HasCompleteArena (authorId, text, out LobbyData data)) {
                AddArena (authorId, data);
                return true;
            }
            return false;
        }

        public LobbyData Pop (ulong id) {
            if (_arenas.ContainsKey (id)) {
                var lobby = _arenas[id];
                _arenas.Remove (id);
                _deleteTasks.Remove (id);
                return lobby;
            }
            return null;
        }

        private async Task WaitForDelete (ulong id) {
            await Task.Delay (_arenaTimeSpan);
            if (_arenas.ContainsKey (id)) {
                _arenas.Remove (id);
                _deleteTasks.Remove (id);
            }

        }

        private void AddArena (ulong authorId, LobbyData data) {
            if (_arenas.ContainsKey (authorId)) {
                _arenas[authorId] = data;
            } else {
                _arenas.Add (authorId, data);
                _deleteTasks.Add (authorId, WaitForDelete (authorId));
            }
        }

        private bool HasCompleteArena (ulong authorId, string text, out LobbyData data) {
            var match = _completeRegex.Match (text);
            if (match.Success) {
                data = new LobbyData ();
                data.roomId = match.Groups[1].Value;
                data.password = match.Groups[2].Value;
                data.ownerId = authorId;
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
    }

    public class LobbyData {
        public string roomId;
        public string password;
        public ulong ownerId;

    }
}