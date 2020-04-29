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
    public class PlayerController : ControllerBase {

        #region HttpGets

        [HttpGet ()]
        public IActionResult GetPlayers () {
            using (var context = new PlayerContext ()) {
                Player[] result = GetAllPlayers (context);

                if (result == null) {
                    return NotFound ();
                }
                return Ok (result);
            }

        }

        [HttpGet ("{id}")]
        public IActionResult GetPlayer (int id) {

            using (var context = new PlayerContext ()) {
                Player result = GetPlayerWithId (id, context);

                if (result == null) {
                    return NotFound ();
                }
                return Ok (result);
            }

        }

        [HttpGet ("{id1}/{id2}/{topic}")]
        public IActionResult GetPlayersMatches (int id1, int id2, string topic) {

            using (var context = new PlayerContext ()) {
                Player firstPlayer = GetPlayerWithId (id1, context);
                if (firstPlayer == null)
                    return NotFound ();

                Player secondPlayer = GetPlayerWithId (id2, context);
                if (secondPlayer == null)
                    return NotFound ();

                Match result = GetPlayerMatches (ref firstPlayer, ref secondPlayer, context, topic, false);
                if (result == null) {
                    return NotFound ();
                }

                return Ok (result);
            }
        }

        [Route ("name")]
        [HttpGet ("{name}")]
        public IActionResult GetPlayerByName (string name) {

            using (var context = new PlayerContext ()) {
                Player[] result = GetPlayersWithName (name, context);
                if (result == null || result.Length == 0) {
                    return NotFound ();
                }
                return Ok ();
            }
        }
        #endregion

        #region HttpPost
        [Route ("addplayer")]
        [HttpPost]
        public IActionResult AddPlayer ([FromBody] Player data) {
            if (data == null) return BadRequest ();
            using (var context = new PlayerContext ()) {
                context.Add<Player> (data);
                context.SaveChanges ();
            }
            return Ok ();
        }

        [Route ("addplayernickname")]
        [HttpPut]
        public IActionResult AddPlayerNickname ([FromBody] Nickname data) {
            if (data == null) return BadRequest ();
            using (var context = new PlayerContext ()) {
                context.Add<Nickname> (data);
                context.SaveChanges ();
            }
            return Ok ();
        }

        [Route ("setmatch")]
        [HttpPost]
        public IActionResult SetPlayersMatch ([FromBody] TwoIds json) {
            int winnerId = json.WinnerId;
            int loserId = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            using (var context = new PlayerContext ()) {
                var winner = GetPlayerWithId (winnerId, context);
                if (winner == null) {
                    return BadRequest ();
                }
                var loser = GetPlayerWithId (loserId, context);
                if (loser == null) {
                    return BadRequest ();
                }

                var result = GetPlayerMatches (ref winner, ref loser, context, topic, true);

                result.PendingFight = true;
                context.SaveChanges ();

                return Ok (result);
            }
        }

        /// <summary>
        /// Finishes a match and sets PendingMath to false.
        /// </summary>
        /// <remarks>
        /// !The winner is considered the first id received.
        /// </remarks>
        /// <param name="id1">The winner's id</param>
        /// <param name="id2">The loser's id</param>
        /// <returns></returns>
        [Route ("completematch")]
        [HttpPost]
        public IActionResult CompletePlayersMatch ([FromBody] TwoIds json) {
            int id1 = json.WinnerId;
            int id2 = json.LoserId;
            string topic = json.Topic ?? Match.DefaultTopic;
            using (var context = new PlayerContext ()) {
                var winner = GetPlayerWithId (id1, context);
                if (winner == null) {
                    return BadRequest ();
                }
                var loser = GetPlayerWithId (id2, context);
                if (loser == null) {
                    return BadRequest ();
                }

                var result = GetPlayerMatches (ref winner, ref loser, context, topic, false);
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

        }
        #endregion

        #region Aux Functions

        private Player[] GetAllPlayers (PlayerContext context) {
            Player[] result = null;

            result = (from p in context.Players select p).ToArray ();

            return result;
        }
        private Player GetPlayerWithId (int id, PlayerContext context) {
            Player result = null;

            result = (from p in context.Players where p.PlayerId == id select p).FirstOrDefault ();

            return result;
        }

        private Player[] GetPlayersWithName (string name, PlayerContext context) {
            Player[] result = null;
            result = (from p in context.Players where p.Name == name select p).ToArray ();
            return result;
        }

        /// <summary>
        /// Finds a match with a player. If no match is found and create is true, one is created.
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="opposing"></param>
        /// <param name="context"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        private Match GetPlayerMatches (ref Player winner, ref Player opposing, PlayerContext context, string topic, bool create) {
            Match result = null;
            if (winner.PlayerMatches == null) {
                winner.PlayerMatches = new List<Match> ();
            }
            if (winner.OpponentMatches == null) {
                winner.OpponentMatches = new List<Match> ();
            }

            int playerId = winner.PlayerId;
            int opposingId = opposing.PlayerId;

            result = (from p in winner.PlayerMatches where p.OpponentPlayerId == opposingId && p.Topic.Equals (topic) select p).FirstOrDefault ();
            if (result == null && create) {
                //We didn't find any match, so we create it.
                result = new Match {
                OpponentPlayerId = opposingId,
                OpponentPlayer = opposing,
                Topic = topic ?? "general"
                };

                winner.PlayerMatches.Add (result);
                winner.OpponentMatches.Add (result);
                context.Matches.Add (result);

            }
            return result;
        }
        #endregion
    }
}