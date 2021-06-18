using System;
using System.ComponentModel.Design.Serialization;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Modules.SavedDataServices;

namespace SmashBotUltimate.Bot.Commands {

    public class TimerCommands : BaseCommandModule {

        public ISavedData<object, TimerData> Timer { get; set; }

        [Command ("temporizador")]
        [Description ("Crea un temporizador para un recordatorio.")]
        private async Task Minutos (CommandContext context, string args, [RemainingText] string description) {
            if (!int.TryParse (args, out int minutes)) return;

            var timerData = new TimerData () { description = description, timeSpan = new System.TimeSpan (0, minutes, 0) };
            timerData.callback +=
                () => context.RespondAsync ($"{context.Member.Mention} se activó la alarma! {timerData.description}");

            Timer.SaveData (GetKey (context), timerData);
            await context.RespondAsync ($"Se agregó el temporizador {context.Member.Mention}!");
        }

        [Command ("terminar")]
        [Description ("Borra un temporizador que se haya creado.")]
        private async Task Cancelar (CommandContext context) {
            var key = GetKey (context);
            if (!Timer.HasData (key)) {
                await context.RespondAsync ("No existe el temporizador");
                return;
            }
            Timer.RemoveData (key);
            await context.RespondAsync ("Se canceló el temporizador");
        }

        private object GetKey (CommandContext context) {
            return new { channel = context.Channel.Id, member = context.Member.Id };
        }
    }
}