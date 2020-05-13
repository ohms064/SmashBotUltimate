using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Bot.Modules.InstructionService;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Bot.Commands {
    public class ReportCommands : BaseCommandModule {

        public PlayerContext DBContext { get; set; }
        public IResultService ResultService { get; set; }
        public IChannelRedirectionService ChannelRedirection { get; set; }

        public PlayerDBService DBConection { get; set; }

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

            await context.ReplyAsync ($"{winner.DisplayName} has ganado! {winner.DisplayName} baneas un escenario, {loser.DisplayName} banea dos y {winner.DisplayName} escoge escenario.");

        }

        /// <summary>
        /// Reporta una victoria sobre targetUser.
        /// </summary>
        /// <param name="targetUser">Loser</param>
        /// <param name="resultStr">Result</param>
        /// <returns></returns>
        [Command ("gane")]
        [Aliases ("derrote", "derroté", "vencí", "venci", "gané")]
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

            var messageValid = ResultService.GetResult (callingUser, targetUser, resultStr, out Result result);

            if (!messageValid) {
                await context.ReplyAsync ($"No se escribió bien el resultado: {resultStr}. Ejemplo: 2-1");
                return;
            }

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

            var hasAdmin = callingUser.Roles.Where (role => role.Name.Contains ("admin")).Count () > 0;
            if (!hasAdmin) {
                var interaction = context.Client.GetInteractivity ();

                await context.RespondAsync ($"{targetUser.Mention} confirma la victoria de {callingUser.DisplayName}. Sólo escribe sí o no.");

                var response = await interaction.WaitForMessageAsync (context.WithPredicate ().ToUser (targetUser).InSameChannel ());

                if (response.TimedOut || response.Result.Content.Contains ("no")) {
                    await context.RespondAsync (
                        $"{callingUser.Mention} no se ha aceptado tu victoria. Revisa si hay  un problema o contacta a un administrador.");
                    return;
                }
            }

            if (DBConection.SetMatch (context.Guild.Id, callingUser, targetUser)) {
                await context.RespondAsync ($"Se ha reportado la victoria de {callingUser.DisplayName}");
            }
            else {
                await context.RespondAsync ($"Ha habido un problema con tu registro!");
            }

        }

    }
}