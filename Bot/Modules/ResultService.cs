using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Models;
namespace SmashBotUltimate.Bot

{

    public interface IResultService {
        bool GetResult (DiscordUser winner, DiscordUser loser, string args, out Result result);
    }

    public class ResultService : IResultService {
        private const string resultPattern = @"\b*(\d{1,2})\D+(\d{1,2})(.*)";

        private const string groupPattern = @".*(\d{1}).*";

        private Regex _resultRegex;

        public ResultService () {
            _resultRegex = new Regex (resultPattern);
        }

        public bool GetResult (DiscordUser winner, DiscordUser loser, string args, out Result result) {
            var resultMatch = _resultRegex.Match (args);
            if (resultMatch.Success) {
                var first = int.Parse (resultMatch.Groups[1].Value); //By this point the regex should ensure a digit
                var second = int.Parse (resultMatch.Groups[2].Value);
                var message = resultMatch.Groups[3].Value;
                var winnerScore = System.Math.Max (first, second);
                var loserScore = System.Math.Min (first, second);

                result = new Result (
                    new PlayerResult (winner, winnerScore),
                    new PlayerResult (loser, loserScore),
                    message
                );
                return true;
            }
            result = null;
            return false;
        }
    }
}