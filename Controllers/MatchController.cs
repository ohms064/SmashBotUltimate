using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
        public IActionResult GetPlayersMatches (int id1, int id2, string topic) {

            if (id1 == id2) {
                return BadRequest ("Ids can't be the same");
            }

            Player firstPlayer = PlayerController.GetPlayerWithId (id1, context, includeMatches : true);
            if (firstPlayer == null)
                return BadRequest ("id1 not found");

            Player secondPlayer = PlayerController.GetPlayerWithId (id2, context);
            if (secondPlayer == null)
                return BadRequest ("id2 not found");

            Match result = MatchController.GetPlayerMatch (ref firstPlayer, ref secondPlayer, context, topic, false);
            if (result == null) {
                return BadRequest ("couldn't find topic");
            }

            return Ok (result);

        }

        [HttpDelete]
        public IActionResult DeletePlayerMatches ([FromBody] OneId json) {
            var player = PlayerController.GetPlayerWithId (json.PlayerId, context, readOnly : false);
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
        public IActionResult SetPlayersMatch ([FromBody] TwoIds json) {
            int firstId = json.WinnerId;
            int secondId = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            var firstPlayer = PlayerController.GetPlayerWithId (firstId, context, includeMatches : true, readOnly : false);
            if (firstPlayer == null) {
                return BadRequest ();
            }
            var secondPlayer = PlayerController.GetPlayerWithId (secondId, context, includeOpponentMatches : true, readOnly : false);
            if (secondPlayer == null) {
                return BadRequest ();
            }
            List<Match> result = new List<Match> ();
            result.Add (MatchController.GetPlayerMatch (ref firstPlayer, ref secondPlayer, context, topic, true));
            result.Add (MatchController.GetPlayerMatch (ref secondPlayer, ref firstPlayer, context, topic, true));

            foreach (var r in result) {
                r.PendingFight = true;
            }
            context.SaveChanges ();

            return Ok (result);

        }

        [Route ("completematch")]
        [HttpPost]
        public IActionResult CompletePlayersMatch ([FromBody] TwoIds json) {
            int id1 = json.WinnerId;
            int id2 = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            var winner = PlayerController.GetPlayerWithId (id1, context, includeMatches : true, readOnly : false);
            if (winner == null) {
                return BadRequest ();
            }
            var loser = PlayerController.GetPlayerWithId (id2, context, readOnly : false);
            if (loser == null) {
                return BadRequest ();
            }

            var winnerResult = MatchController.GetPlayerMatch (ref winner, ref loser, context, topic, false);
            var loserResult = MatchController.GetPlayerMatch (ref loser, ref winner, context, topic, false);

            if (winnerResult == null || loserResult == null) {
                return BadRequest ("Match has not been set to start.");
            }
            if (!winnerResult.PendingFight || !loserResult.PendingFight) {
                return BadRequest ("No match is pending");
            }

            winnerResult.WinCount++;
            winnerResult.PendingFight = false;
            loserResult.PendingFight = false;
            context.Update<Match> (winnerResult);
            context.Update<Match> (loserResult);
            context.SaveChanges ();
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
        public static Match GetPlayerMatch (ref Player local, ref Player opposing, PlayerContext context, string topic, bool create) {
            Match result = null;
            if (local.PlayerMatches == null) {
                local.PlayerMatches = new List<Match> ();
            }
            if (opposing.OpponentMatches == null) {
                opposing.OpponentMatches = new List<Match> ();
            }

            int playerId = local.PlayerId;
            int opposingId = opposing.PlayerId;

            result = (from p in local.PlayerMatches where p.OpponentPlayerId == opposingId && p.Topic.Equals (topic) select p).FirstOrDefault ();
            if (result == null && create) {
                //We didn't find any match, so we create it.
                result = new Match {
                OpponentPlayerId = opposingId,
                OpponentPlayer = opposing,
                Topic = topic ?? "general"
                };

                local.PlayerMatches.Add (result);
                opposing.OpponentMatches.Add (result);
                context.Matches.Add (result);
                context.Update<Player> (local);
                context.Update<Player> (opposing);

            }
            return result;
        }
    }
}