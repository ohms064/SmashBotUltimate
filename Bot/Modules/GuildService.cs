using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SmashBotUltimate.Bot.Modules {

    public interface IGuildService {
        DiscordRole FindRole (CommandContext context, string roleName);
        DiscordChannel FindChannel (CommandContext context, string channelName);
        DiscordChannel FindTextChannel (CommandContext context, string channelName);
        Task SendMessageToTextChannel (DiscordChannel channel, string message, DiscordRole toRole = null);
        string GetEntityNames<T> (ICollection<T> entities);
        string GetEntityNames<T> (IEnumerable<T> entities);
    }

    public class GuildService : IGuildService {
        public DiscordRole FindRole (CommandContext context, string roleName) {
            var result = (from role in context.Guild.Roles where role.Value.Name.Equals (roleName) select role).FirstOrDefault ();
            return result.Value;
        }

        public DiscordChannel FindChannel (CommandContext context, string channelName) {
            var result = (from channel in context.Guild.Channels where channel.Value.Name.Equals (channelName) select channel).FirstOrDefault ();
            return result.Value;
        }

        public DiscordChannel FindTextChannel (CommandContext context, string channelName) {
            var result = (from channel in context.Guild.Channels where (channel.Value.Name.Equals (channelName) && channel.Value.Type == ChannelType.Text) select channel).FirstOrDefault ();
            return result.Value;
        }

        public async Task SendMessageToTextChannel (DiscordChannel channel, string message, DiscordRole toRole = null) {
            if (toRole != null && toRole.IsMentionable) {
                await channel.SendMessageAsync ($"{toRole.Mention} {message}");
            } else {
                await channel.SendMessageAsync (message);
            }
        }

        public string GetEntityNames<T> (ICollection<T> entities) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder ();
            foreach (var e in entities) {
                builder.Append ($"{e} ");
            }
            return builder.ToString ();
        }

        public string GetEntityNames<T> (IEnumerable<T> entities) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder ();
            foreach (var e in entities) {
                builder.Append ($"{e} ");
            }
            return builder.ToString ();
        }
    }
}