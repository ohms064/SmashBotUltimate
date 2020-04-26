using DSharpPlus.Entities;
namespace SmashBotUltimate.Bot.Models {
    public class Result {

        public readonly PlayerResult winner, loser;

        public readonly string message;

        public Result (PlayerResult winner, PlayerResult loser, string message) {
            this.winner = winner;
            this.loser = loser;
            this.message = message;
        }
    }

    public class PlayerResult {

        public readonly DiscordUser user;
        public readonly int score;

        public PlayerResult (DiscordUser name, int score) {
            this.user = name;
            this.score = score;
        }

        public override string ToString () {
            return $"{user.Mention}: {score}";
        }
    }
}