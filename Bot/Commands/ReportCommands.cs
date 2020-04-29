using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Bot.Modules.InstructionService;

namespace SmashBotUltimate.Bot.Commands {
    public class ReportCommands : BaseCommandModule {
        public IResultService ResultService { get; set; }
        public IChannelRedirectionService ChannelRedirection { get; set; }

        public IRandomUtilitiesService RandomService { get; set; }

        public IGuildService GuildService { get; set; }

        public IInteractionService<CoinTossResult, string> CoinTossService { get; set; }

        private const string Head = "heads",
            Tails = "tails";

        public ReportCommands (IResultService result, IChannelRedirectionService channel, IGuildService guild) {
            ResultService = result;
            ChannelRedirection = channel;
            GuildService = guild;
        }

        [Command ("batalla")]
        [Aliases ("pelea", "inicia")]
        [Description ("Inicia el procesos de pelea contra un oponente")]

        public async Task BeginBattle (CommandContext context, [Description ("El usuario al que retarás")] DiscordMember adversary, [RemainingText] string text) {
            var callingMember = context.Member;

            if (callingMember == adversary) {
                return;
            }

            var startingMember = RandomService.PickOne (callingMember, adversary);
            var otherMember = startingMember == callingMember ? adversary : callingMember;

            var message = $"{callingMember.Mention} reta a {adversary.Mention}.";
            await context.ReplyAsync (message);

            var startingWon = await CoinTossService.BeginInteraction (context, startingMember);
            var winner = startingWon.successGuess ? startingMember : otherMember;
            var loser = startingWon.successGuess ? otherMember : startingMember;

            await context.ReplyAsync ($"{winner.DisplayName} has ganado! {winner.DisplayName} baneas un escenario, {loser.DisplayName} banea dos y {winner.DisplayName} escoge escenarios.");

        }

        /// <summary>
        /// Reporta una victoria sobre targetUser.
        /// </summary>
        /// <param name="targetUser">Loser</param>
        /// <param name="resultStr">Result</param>
        /// <returns></returns>
        [Command ("gane")]
        [Description ("Reporta una victoria sobre un jugador.")]
        public async Task Victoria (CommandContext context, [Description ("El usuario al que le ganaste")] DiscordMember targetUser = null, [RemainingText] string resultStr = null) {
            var callingUser = context.Member;
            if (callingUser == targetUser) {
                return;
            }

            if (targetUser == null) {
                await context.ReplyAsync ("Faltó escribir el oponente.");
                return;
            }

            if (targetUser.IsBot) {
                await context.ReplyAsync ("No puedes reportar una victoria contra un bot!");
                return;
            }

            if (string.IsNullOrWhiteSpace (resultStr)) {
                await context.ReplyAsync ("Faltó escribir el resultado");
                return;
            }

            if (ResultService.GetResult (callingUser, targetUser, resultStr, out Result result)) {
                var generalMessage = $"Resultado: \n\t{result.winner}\t{result.loser}. {result.message}";
                await targetUser.SendMessageAsync ($"El usuario {callingUser.Mention} reportó una victoria {result.winner.score}-{result.loser.score} contra ti.");

                var channelName = ChannelRedirection.GetRedirectedChannel (context.Channel.Name);

                var channel = GuildService.FindTextChannel (context, channelName);
                var admins = GuildService.FindRole (context, "admin");

                if (channel == null) {
                    var roles = callingUser.Roles;
                    var roleText = GuildService.GetEntityNames (roles);
                    generalMessage = $"{generalMessage}\nCanal: {context.Channel.Name} Roles: {roleText}";

                    channelName = ChannelRedirection.TargetChannelName;
                    channel = GuildService.FindTextChannel (context, channelName);

                    if (channel == null) {
                        await context.ReplyAsync ($"{generalMessage}");
                        return;
                    }
                }

                await GuildService.SendMessageToTextChannel (channel, generalMessage, admins);
                await context.ReplyAsync ($"{generalMessage}");
            } else {
                await context.ReplyAsync ($"No se escribió bien el resultado: {resultStr}. Ejemplo: 2-1");
            }

        }

    }
}