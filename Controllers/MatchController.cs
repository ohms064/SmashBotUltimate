using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using SmashBotUltimate.Models;
namespace SmashBotUltimate.Controllers {
    [Route ("[controller]")]
    [ApiController]
    public class MatchController : ControllerBase {

        public PlayerContext context { get; set; }
        public MatchController (PlayerContext context) {
            this.context = context;
        }

        [HttpGet ("user")]
        public async Task<IActionResult> GetPlayersMatches (ulong id1, ulong id2, string topic, ulong guildId) {

            if (id1 == id2) {
                return BadRequest ("Ids can't be the same");
            }

            Player firstPlayer = await PlayerController.GetPlayerWithId (id1, context, includeMatches : true);
            if (firstPlayer == null)
                return BadRequest ("id1 not found");

            Player secondPlayer = await PlayerController.GetPlayerWithId (id2, context);
            if (secondPlayer == null)
                return BadRequest ("id2 not found");

            Match result = await MatchController.GetPlayerMatch (firstPlayer, secondPlayer, context, topic, guildId);
            if (result == null) {
                return BadRequest ("couldn't find topic");
            }

            return Ok (result);

        }

        [HttpDelete]
        public async Task<IActionResult> DeletePlayerMatches ([FromBody] OneId json) {
            var player = await PlayerController.GetPlayerWithId (json.PlayerId, context, readOnly : false);
            if (player == null) {
                return BadRequest ("The id doesn't have an associated player.");
            }

            return Ok ();

        }

        /// <summary>
        /// Finds a match with a player. If no match is found and create is true, one is created.
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="opposing"></param>
        /// <param name="context"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        [Route ("setmatch")]
        [HttpPost]
        public async Task<IActionResult> SetPlayersMatch ([FromBody] TwoIds json) {
            ulong firstId = json.WinnerId;
            ulong secondId = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            ulong guildId = json.GuildId;
            var firstPlayer = await PlayerController.GetPlayerWithId (firstId, context, includeMatches : true, readOnly : false);
            if (firstPlayer == null) {
                return BadRequest ();
            }
            var secondPlayer = await PlayerController.GetPlayerWithId (secondId, context, includeOpponentMatches : true, readOnly : false);
            if (secondPlayer == null) {
                return BadRequest ();
            }
            List<Match> result = new List<Match> ();
            result.Add (await MatchController.GetPlayerMatch (firstPlayer, secondPlayer, context, topic, guildId));
            result.Add (await MatchController.GetPlayerMatch (secondPlayer, firstPlayer, context, topic, guildId));

            foreach (var r in result) {
                r.PendingFight = true;
            }
            context.SaveChanges ();

            return Ok (result);

        }

        [Route ("completematch")]
        [HttpPost]
        public async Task<IActionResult> CompletePlayersMatch ([FromBody] TwoIds json) {
            ulong id1 = json.WinnerId;
            ulong id2 = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            ulong guildId = json.GuildId;
            var winner = await PlayerController.GetPlayerWithId (id1, context, includeMatches : true, readOnly : false);
            if (winner == null) {
                return BadRequest ();
            }
            var loser = await PlayerController.GetPlayerWithId (id2, context, readOnly : false);
            if (loser == null) {
                return BadRequest ();
            }

            var results = await GetCompletePlayerMatch (context, winner, loser, topic, guildId);

            var winnerResult = results[0];
            var loserResult = results[1];

            if (winnerResult == null || loserResult == null) {
                return BadRequest ("Match has not been set to start.");
            }
            if (!winnerResult.PendingFight || !loserResult.PendingFight) {
                return BadRequest ("No match is pending");
            }

            await CompletePlayerMatch (context, winnerResult, loserResult);
            return Ok (winnerResult);

        }

        [Route ("all")]
        [HttpDelete]
        public IActionResult DeleteAllMatches () {
            foreach (var match in context.Matches.ToList ()) {
                context.Matches.Remove (match);
            }
            context.SaveChanges ();
            return Ok ();
        }

        //TODO: Remove context
        public static Match[] GetPlayerMatches (ref Player winner, PlayerContext context) {
            return (from m in winner.PlayerMatches select m).ToArray ();
        }

        /// <summary>
        /// Gets a player match.true If create is true requires that the player has been configured as writable and 
        /// local has PlayerMatches and opposing has OpponentMatches. If create is false only local must include PlayerMatches
        /// </summary>
        /// <param name="local"></param>
        /// <param name="opposing"></param>
        /// <param name="context"></param>
        /// <param name="topic"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static async Task<Match> GetPlayerMatch (Player local, Player opposing, PlayerContext context, string topic, ulong guildId, bool disconnected = false) {
            Match result = null;
            if (local.PlayerMatches == null) {
                local.PlayerMatches = new List<Match> ();
            }
            if (opposing.OpponentMatches == null) {
                opposing.OpponentMatches = new List<Match> ();
            }

            ulong playerId = local.PlayerId;
            ulong opposingId = opposing.PlayerId;

            result = (from p in local.PlayerMatches where p.OpponentPlayerId == opposingId && p.Topic.Equals (topic) select p).FirstOrDefault (m => m.GuildId == guildId);
            if (result == null) {
                //We didn't find any match, so we create it.
                result = await CreateMatch (context, local, opposing, topic, guildId, disconnected);
            }
            return result;
        }

        public static async Task<Match[]> GetCompletePlayerMatch (PlayerContext context, Player first, Player second, string topic, ulong guildId) {
            return new Match[] {
                await GetPlayerMatch (first, second, context, topic, guildId),
                    await GetPlayerMatch (second, first, context, topic, guildId),
            };
        }

        public static async Task StartPlayerMatch (PlayerContext context, Match first, Match second) {
            first.PendingFight = true;
            second.PendingFight = true;
            await context.SaveChangesAsync ();
        }

        public static async Task CompletePlayerMatch (PlayerContext context, Match winnerMatch, Match loserMatch) {
            winnerMatch.WinCount++;
            winnerMatch.PendingFight = false;
            loserMatch.PendingFight = false;
            winnerMatch.LastMatch = DateTime.Today;
            context.Update<Match> (winnerMatch);
            context.Update<Match> (loserMatch);
            await context.SaveChangesAsync ();
        }

        private static async Task<Match> CreateMatch (PlayerContext context, Player local, Player opposing, string topic, ulong guildId, bool disconnected) {
            var result = new Match {
                OpponentPlayerId = opposing.PlayerId,
                OpponentPlayer = opposing,
                PendingFight = true,
                Topic = topic ?? "general",
                GuildId = guildId
            };

            local.PlayerMatches.Add (result);
            opposing.OpponentMatches.Add (result);
            context.Matches.Add (result);
            if (disconnected) {
                context.Update (local);
                context.Update (opposing);
            }
            await context.SaveChangesAsync ();
            return result;

        }
    }
}