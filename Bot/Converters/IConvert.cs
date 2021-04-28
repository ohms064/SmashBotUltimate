using DSharpPlus.CommandsNext;
namespace SmashBotUltimate.Bot.Converters {
    public interface IConvert<T> {
        bool Convert (string text, CommandContext context, out T result);
    }
}