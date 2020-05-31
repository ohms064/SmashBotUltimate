using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
namespace SmashBotUltimate.Bot.Modules {

    public interface ILobbyService {
        List<LobbyData> GetArenas ();
        LobbyData Pop (ulong id);
    }

    public class LobbyService : ILobbyService {
        private Dictionary<ulong, LobbyData> _arenas;
        private Dictionary<ulong, Task> _deleteTasks;
        private const string arenaRegex = @"\b(\w{5})\s\S\s(\d+)\b";
        private Regex _regex;

        private const string listenChannel = "retas";

        private TimeSpan _arenaTimeSpan;

        public LobbyService () {
            _arenas = new Dictionary<ulong, LobbyData> ();
            _deleteTasks = new Dictionary<ulong, Task> ();
            _regex = new Regex (arenaRegex);
            _arenaTimeSpan = new TimeSpan (5, 0, 0); //5 horas
        }

        public List<LobbyData> GetArenas () {
            return new List<LobbyData> (_arenas.Values);
        }

        public LobbyData Pop (ulong id) {
            if (_arenas.ContainsKey (id)) {
                var lobby = _arenas[id];
                _arenas.Remove (id);
                return lobby;
            }
            return null;
        }

        public async Task OnMessage (MessageCreateEventArgs args) {
            if (!args.Channel.Name.Equals (listenChannel)) return;
            var authorId = args.Author.Id;
            if (HasArena (authorId, args.Message.Content, out LobbyData data)) {

                if (_arenas.ContainsKey (authorId)) {
                    _arenas[authorId] = data;
                    _deleteTasks[authorId].Dispose ();
                    _deleteTasks[authorId] = WaitForDelete (authorId);
                } else {
                    _arenas.Add (authorId, data);
                    _deleteTasks.Add (authorId, WaitForDelete (authorId));
                }
                await args.Channel.SendMessageAsync ("Se agregó la sala!");
            }
        }

        private async Task WaitForDelete (ulong id) {
            await Task.Delay (_arenaTimeSpan);
            if (_arenas.ContainsKey (id)) {
                _arenas.Remove (id);
                _deleteTasks.Remove (id);
            }

        }

        private bool HasArena (ulong authorId, string text, out LobbyData data) {
            var match = _regex.Match (text);
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
    }

    public class LobbyData {
        public string roomId;
        public string password;
        public ulong ownerId;

    }
}