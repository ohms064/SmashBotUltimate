using System.Text.RegularExpressions;
namespace SmashBotUltimate.Bot.Modules {

    public interface IChannelRedirectionService {
        string TargetChannelName { get; set; }
        string OriginChannelName { get; set; }
        string GetRedirectedChannel (string origin);
    }
    public class ChannelRedirectionService : IChannelRedirectionService {
        private const string ChannelPattern = @"-(\S+)";
        public string OriginChannelName { get; set; }
        public string TargetChannelName { get; set; }

        private Regex _channelRegex;

        public ChannelRedirectionService () {
            OriginChannelName = "pool";
            TargetChannelName = "resultados";
            _channelRegex = new Regex (ChannelPattern);
        }

        public string GetRedirectedChannel (string origin) {
            var match = _channelRegex.Match (origin);
            if (match.Success) {
                var group = match.Groups[1].Value;
                return $"{TargetChannelName}-{group}";
            }
            return "";
        }

    }
}