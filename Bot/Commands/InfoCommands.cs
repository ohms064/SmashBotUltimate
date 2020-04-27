using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;
namespace SmashBotUltimate.Bot.Commands {
    public class InfoCommands : BaseCommandModule {

        public const string Starters = "Starters: Battlefield, Final Destination, Smashville, Town and city, Pokemon Stadium 2";
        public const string Counterpicks = "Counterpicks: Kalos Pokemon League, Yoshi Story";
        //TODO: Grab these from a file or from database.

        [Command ("starters")]
        public async Task ShowStarters (CommandContext context) {
            await context.ReplyAsync (Starters);
        }

        [Command ("counterpicks")]
        public async Task ShowCounterpicks (CommandContext context) {
            await context.ReplyAsync (Counterpicks);
        }

        [Command ("stages")]
        public async Task ShowStages (CommandContext context) {
            await ShowStarters (context);
            await ShowCounterpicks (context);
        }
    }
}