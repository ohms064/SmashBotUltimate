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

        [HttpGet ("{id1}/{id2}/{topic}")]
        public IActionResult GetPlayersMatches (int id1, int id2, string topic) {

            Player firstPlayer = PlayerController.GetPlayerWithId (id1, context);
            if (firstPlayer == null)
                return NotFound ();

            Player secondPlayer = PlayerController.GetPlayerWithId (id2, context);
            if (secondPlayer == null)
                return NotFound ();

            Match result = MatchController.GetPlayerMatch (ref firstPlayer, ref secondPlayer, context, topic, false);
            if (result == null) {
                return NotFound ();
            }

            return Ok (result);

        }

        [HttpDelete]
        public IActionResult DeletePlayerMatches ([FromBody] OneId json) {
            var player = PlayerController.GetPlayerWithId (json.PlayerId, context);
            if (player == null) {
                return BadRequest ("The id doesn't have an associated player.");
            }

            return Ok ();

        }

        [Route ("setmatch")]
        [HttpPost]
        public IActionResult SetPlayersMatch ([FromBody] TwoIds json) {
            int winnerId = json.WinnerId;
            int loserId = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            var winner = PlayerController.GetPlayerWithId (winnerId, context);
            if (winner == null) {
                return BadRequest ();
            }
            var loser = PlayerController.GetPlayerWithId (loserId, context);
            if (loser == null) {
                return BadRequest ();
            }

            var result = MatchController.GetPlayerMatch (ref winner, ref loser, context, topic, true);

            result.PendingFight = true;
            context.SaveChanges ();

            return Ok (result);

        }

        [Route ("completematch")]
        [HttpPost]
        public IActionResult CompletePlayersMatch ([FromBody] TwoIds json) {
            int id1 = json.WinnerId;
            int id2 = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            var winner = PlayerController.GetPlayerWithId (id1, context);
            if (winner == null) {
                return BadRequest ();
            }
            var loser = PlayerController.GetPlayerWithId (id2, context);
            if (loser == null) {
                return BadRequest ();
            }

            var result = MatchController.GetPlayerMatch (ref winner, ref loser, context, topic, false);
            if (result == null) {
                return BadRequest ("Match has not been set to start.");
            }
            if (!result.PendingFight) {
                return BadRequest ("No match is pending");
            }
            result.WinCount++;
            result.PendingFight = false;
            context.Update<Match> (result);
            context.SaveChanges ();
            return Ok (result);

        }

        public static Match[] GetPlayerMatches (ref Player winner, PlayerContext context) {
            return (from m in winner.PlayerMatches select m).ToArray ();
        }

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