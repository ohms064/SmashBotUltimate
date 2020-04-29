using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SmashBotUltimate.Bot.Modules.InstructionService {
    public interface IInteractionService<T, W> {
        Task<T> BeginInteraction (CommandContext context, DiscordMember calling);

        Task<W> SimpleAction (CommandContext context);
    }
}