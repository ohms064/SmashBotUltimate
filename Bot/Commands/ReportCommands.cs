using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;

namespace SmashBotUltimate.Bot.Commands {
    public class ReportCommands : BaseCommands {
        public IResultService ResultService { get; set; }
        public IChannelRedirectionService ChannelRedirection { get; set; }

        public IRandomUtilitiesService RandomService { get; set; }

        public IGuildService GuildService { get; set; }

        private const string Head = "heads",
            Tails = "tails";

        public ReportCommands (IResultService result, IChannelRedirectionService channel, IGuildService guild) {
            ResultService = result;
            ChannelRedirection = channel;
            GuildService = guild;
        }

        [Command ("batalla")]

        public async Task BeginBattle (CommandContext context, [Description ("El usuario al que retarás")] DiscordMember adversary, [RemainingText] string text) {
            var callingMember = context.Member;
            var interactivity = context.Client.GetInteractivity ();

            var startingMember = RandomService.PickOne (callingMember, adversary);
            var otherMember = startingMember == callingMember ? adversary : callingMember;
            var message = $"{callingMember.Mention} reta a {adversary.Mention}.\n{startingMember.Mention} escribe Heads o Tails";
            await ReplyAsync (context, message);
            //?A lo mejor usar otro bot existente?

            var resultMessage = await interactivity.WaitForMessageAsync (SameChannelResponse (context, (x) => x.Author == startingMember));

            if (resultMessage.TimedOut) {
                await ReplyAsync (context, $"¡No hubo respuesta de {startingMember.Mention}! Vuelve a intenarlo.");
                return;
            }
            var coinResult = RandomService.PickOne (Head, Tails);
            var expectedResult = resultMessage.Result.Content.Trim ().ToLower (); //Limpiamos el string de caracteres de espacio

            if (!expectedResult.StartsWith (Tails[0]) && !expectedResult.StartsWith (Head[0])) {
                await ReplyAsync (context, $"{startingMember.Mention} no elegiste bien!");
                return;
            }

            await ReplyAsync (context, $"Tirando... Salió {coinResult}.");

            var startingWon = resultMessage.Result.Content.StartsWith (coinResult[0]); //solo comparamos la primera letra, por si hay un typo.

            var winner = startingWon ? startingMember : otherMember;
            var loser = startingWon ? otherMember : startingMember;
            await ReplyAsync (context, $"{winner.DisplayName} has ganado! {winner.DisplayName} baneas un escenario, {loser.DisplayName} banea dos y {winner.DisplayName} escoge escenarios.");
            context.Client.GetCommandsNext ().FindCommand <
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
            if (callingUser == null) {
                await ReplyAsync (context, "WTF, quién me escribió");
                return;
            }

            if (targetUser == null) {
                await ReplyAsync (context, "Faltó escribir el oponente.");
                return;
            }

            if (targetUser.IsBot) {
                await ReplyAsync (context, "No puedes reportar una victoria contra un bot!");
                return;
            }

            if (string.IsNullOrWhiteSpace (resultStr)) {
                await ReplyAsync (context, "Faltó escribir el resultado");
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
                        await ReplyAsync (context, $"{generalMessage}");
                        return;
                    }
                }

                await GuildService.SendMessageToTextChannel (channel, generalMessage, admins);
                await ReplyAsync (context, $"{generalMessage}");
            } else {
                await ReplyAsync (context, $"No se escribió bien el resultado: {resultStr}. Ejemplo: 2-1");
            }

        }

    }
}