using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;

namespace SmashBotUltimate.Bot.Commands {
    public class ReportCommands : BaseCommands {
        public IResultService ResultService { get; set; }
        public IChannelRedirectionService ChannelRedirection { get; set; }

        public IGuildService GuildService { get; set; }

        public ReportCommands (IResultService result, IChannelRedirectionService channel, IGuildService guild) {
            ResultService = result;
            ChannelRedirection = channel;
            GuildService = guild;
        }

        /// <summary>
        /// Reporta una victoria sobre targetUser.
        /// </summary>
        /// <param name="targetUser">Loser</param>
        /// <param name="resultStr">Result</param>
        /// <returns></returns>
        [Command ("gane")]
        [Description ("Reporta una victoria sobre un jugador.")]
        public async Task Victoria (CommandContext Context, [Description ("El usuario al que le ganaste")] DiscordMember targetUser = null, [RemainingText] string resultStr = null) {
            var callingUser = Context.Member;
            if (callingUser == null) {
                await ReplyAsync (Context, "WTF, quién me escribió");
                return;
            }

            if (targetUser == null) {
                await ReplyAsync (Context, "Faltó escribir el oponente.");
                return;
            }

            if (targetUser.IsBot) {
                await ReplyAsync (Context, "No puedes reportar una victoria contra un bot!");
                return;
            }

            if (string.IsNullOrWhiteSpace (resultStr)) {
                await ReplyAsync (Context, "Faltó escribir el resultado");
                return;
            }

            if (ResultService.GetResult (callingUser, targetUser, resultStr, out Result result)) {
                var generalMessage = $"Resultado: \n\t{result.winner}\t{result.loser}. {result.message}";
                await targetUser.SendMessageAsync ($"El usuario {callingUser.Mention} reportó una victoria {result.winner.score}-{result.loser.score} contra ti.");

                var channelName = ChannelRedirection.GetRedirectedChannel (Context.Channel.Name);

                var channel = GuildService.FindTextChannel (Context, channelName);
                var admins = GuildService.FindRole (Context, "admin");

                if (channel == null) {
                    var roles = callingUser.Roles;
                    var roleText = GuildService.GetEntityNames (roles);
                    generalMessage = $"{generalMessage}\nCanal: {Context.Channel.Name} Roles: {roleText}";

                    channelName = ChannelRedirection.TargetChannelName;
                    channel = GuildService.FindTextChannel (Context, channelName);

                    if (channel == null) {
                        await ReplyAsync (Context, $"{generalMessage}");
                        return;
                    }
                }

                await GuildService.SendMessageToTextChannel (channel, generalMessage, admins);
                await ReplyAsync (Context, $"{generalMessage}");
            } else {
                await ReplyAsync (Context, $"No se escribió bien el resultado: {resultStr}. Ejemplo: 2-1");
            }

        }

    }
}