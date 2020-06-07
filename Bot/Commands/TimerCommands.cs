using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Modules.SavedDataServices;

namespace SmashBotUltimate.Bot.Commands {
    [Group ("alarma")]
    public class TimerCommands : BaseCommandModule {

        public ISavedData<string, TimerData> Timer { get; set; }

        [Command ("iniciar")]
        private async Task Minutos (CommandContext context, string args, [RemainingText] string description) {
            if (!int.TryParse (args, out int minutes)) return;

            var timerData = new TimerData () { description = description, timeSpan = new System.TimeSpan (0, minutes, 0) };
            timerData.callback +=
                () => context.RespondAsync ($"{context.Member.Mention} se activó la alarma! {timerData.description}");

            Timer.SaveData (context.Member.Mention, timerData);
            await context.RespondAsync ($"Se agregó el temporizador {context.Member.Mention}!");
        }

        [Command ("cancelar")]
        [Aliases ("terminar")]
        private async Task Cancelar (CommandContext context) {
            if (!Timer.HasData (context.Member.Mention)) return;
            Timer.RemoveData (context.Member.Mention);
            await context.RespondAsync ("Se canceló la alarma");
        }
    }
}