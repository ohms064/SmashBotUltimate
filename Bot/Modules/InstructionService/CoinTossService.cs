using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Extensions;
namespace SmashBotUltimate.Bot.Modules.InstructionService {
    public class CoinTossService : IInteractionService<CoinTossResult, string> {

        public IRandomUtilitiesService RandomService { get; set; }
        public const string Head = "heads";
        public const string Tails = "tails";

        private readonly int _maxAttempts;

        public CoinTossService (int attempts, IRandomUtilitiesService randomService) {
            _maxAttempts = attempts;
            RandomService = randomService;

            if (_maxAttempts < 1) {
                _maxAttempts = 1;
            }
        }

        public async Task<string> SimpleAction (CommandContext context) {
            return await CoinToss (context);
        }

        private async Task<string> CoinToss (CommandContext context) {
            var coinResult = RandomService.PickOne (Head, Tails);
            await context.ReplyAsync ($"Tirando... Salió {coinResult}.");
            return coinResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="calling"></param>
        /// <param name="args">Heads or Tails</param>
        /// <returns></returns>
        public async Task<CoinTossResult> BeginInteraction (CommandContext context, DiscordMember calling) {
            return await CoinTossInteraction (context, calling, context.Client.GetInteractivity (), 0);
        }

        private async Task<CoinTossResult> CoinTossInteraction (CommandContext context, DiscordMember calling, InteractivityExtension interactivity, int attempt) {
            if (attempt == _maxAttempts) {
                await context.ReplyAsync ($"¡{calling.Mention} alcanzaste el máximo número de intentos!");
                return null;
            }
            var message = $"{calling.Mention} escribe Heads o Tails";
            await context.ReplyAsync (message);

            var resultMessage = await interactivity.WaitForMessageAsync (context.WithPredicate ().InSameChannel ().SameUser ());

            if (resultMessage.TimedOut) {
                await context.ReplyAsync ($"¡No hubo respuesta de {calling.Mention}! Vuelve a intenarlo.");
                return null;
            }

            //TODO: Validate and extract the answer with regex instead of just checking it.
            if (UserInputError (resultMessage.Result.Content)) {
                //await context.ReplyAsync ($"{calling.Mention} no elegiste bien!");
                return await CoinTossInteraction (context, calling, interactivity, attempt + 1);
            }

            var coinResult = await CoinToss (context);

            var startingWon = resultMessage.Result.Content.StartsWith (coinResult[0]); //solo comparamos la primera letra, por si hay un typo.
            return new CoinTossResult (startingWon);
        }

        private bool UserInputError (string answer) {
            //We process the string to eliminate accidental spaces and make it not case sensitive.
            var expectedResult = answer.Trim ().ToLower ();
            return !expectedResult.StartsWith (Tails[0]) && !expectedResult.StartsWith (Head[0])
            //&& !expectedResult.Contains (Head) && !expectedResult.Contains (Tails)
            ;
        }

    }

    public class CoinTossResult {
        public readonly bool successGuess;

        public CoinTossResult (bool guessed) {
            successGuess = guessed;
        }
    }
}