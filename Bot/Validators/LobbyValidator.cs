using System.Text.RegularExpressions;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Validators {
    public class LobbyValidator {

        private const string arenaIdPattern = @"(\w{5})";
        private const string arenaPassPattern = @"(\d{1,8})";
        private const string arenaCompletePattern = @"(\w{5})\s?(/|-)\s?(\d{1,8})(.*)";

        private Regex _completeRegex, _passRegex, _idRegex;

        public LobbyValidator () {

            _passRegex = new Regex (arenaPassPattern);
            _idRegex = new Regex (arenaIdPattern);
            _completeRegex = new Regex (arenaCompletePattern);
        }
        public bool IsPassword (string args) {
            var match = _passRegex.Match (args);
            return match.Success;
        }
        public bool IsLobby (string args) {
            var match = _idRegex.Match (args);
            return match.Success;
        }

        public bool IsComplete (string args) {
            var match = _completeRegex.Match (args);
            return match.Success;
        }

        public bool IsLobby (string args, out Lobby lobby) {
            var match = _idRegex.Match (args);
            lobby = new Lobby ();
            if (match.Success) {
                lobby.RoomId = match.Groups[1].Value;
                return true;
            }
            return false;
        }

        public bool IsComplete (string args, out Lobby lobby) {
            var match = _completeRegex.Match (args);
            lobby = new Lobby ();
            var comment = match.Groups[3].Value;
            comment = string.IsNullOrWhiteSpace (comment) ? "" : comment;
            if (match.Success) {
                lobby.RoomId = match.Groups[1].Value;
                lobby.Password = match.Groups[3].Value;
                lobby.Comment = comment;
                return true;
            }
            return false;
        }
    }
}