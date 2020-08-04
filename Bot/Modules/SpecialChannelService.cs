using System.Linq;
using DSharpPlus.Entities;
namespace SmashBotUltimate.Bot.Modules {
    public static class SpecialChannelService {
        public const string Global = "global";

        public static string[] SpecialChannels = { Global };

        public static bool IsSpecialChannel (this DiscordChannel channel) {
            return SpecialChannels.Contains (channel.Name);
        }
    }
}