using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SmashBotUltimate.Bot.Modules.InstructionService {
    public interface IInteractionService<T> {
        Task<T> BeginInteraction (CommandContext context, DiscordMember calling);
    }
}