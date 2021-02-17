using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Validators;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Bot.Converters {
    public class LobbyConverter : IConvert<Lobby> {

        private LobbyValidator _validator;

        public LobbyConverter () {
            _validator = new LobbyValidator ();
        }

        public bool Convert (string value, CommandContext context, out Lobby lobby) {
            var args = value.Split (' ');

            lobby = new Lobby ();

            if (!_validator.IsLobby (args[0])) {
                return false;
            }

            lobby.RoomId = args[0];
            int remainder = 1;
            if (args.Length > 1 && _validator.IsPassword (args[1])) {
                remainder++;
                lobby.Password = args[1];
            }
            else {
                lobby.Password = "";
            }

            StringBuilder strBldr = new StringBuilder ();
            int count = 0;
            const int maxChars = 256;
            for (int i = remainder; i < args.Length; i++) {
                count += args[i].Length;
                if (count > maxChars) break;
                strBldr.Append ($"{args[i]} ");
            }
            lobby.Comment = strBldr.ToString ();

            lobby.GuildId = context.Guild.Id;
            lobby.ChannelId = context.Channel.Id;
            lobby.OwnerId = context.Member.Id;
            lobby.PublishTime = context.Message.Timestamp;

            return true;
        }
    }
}