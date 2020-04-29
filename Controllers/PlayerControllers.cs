using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Controllers {

    [Route ("[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase {

        public PlayerContext Context { get; set; }
        public PlayerController (PlayerContext context) {
            Context = context;
        }

        #region HttpGets

        [HttpGet ()]
        public IActionResult GetPlayers () {
            Player[] result = GetAllPlayers (Context);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [HttpGet ("{id}")]
        public IActionResult GetPlayer (int id) {

            Player result = GetPlayerWithId (id, Context);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [Route ("name")]
        [HttpGet ("{name}")]
        public IActionResult GetPlayerByName (string name) {

            Player result = GetPlayerWithName (name, Context);
            if (result == null) {
                return NotFound ();
            }
            return Ok ();

        }
        #endregion

        #region HttpPost
        [Route ("addplayer")]
        [HttpPost]
        public IActionResult AddPlayer ([FromBody] Player data) {
            if (data == null) return BadRequest ();

            var existingPlayer = GetPlayerWithName (data.Name, Context);

            if (existingPlayer != null) {
                var receivingGuild = data.GuildPlayers.FirstOrDefault ();
                receivingGuild.PlayerId = existingPlayer.PlayerId;
                receivingGuild.Player = existingPlayer;

                if (receivingGuild != null && !existingPlayer.HasGuildId (receivingGuild.GuildId)) {
                    existingPlayer.GuildPlayers.Add (receivingGuild);

                    Context.Update<Player> (existingPlayer);
                } else {
                    return BadRequest ("Received player already exists but no new guild was provided or guild already exists");
                }
            } else {
                try {
                    Context.Add<Player> (data);
                } catch (Exception e) {
                    return BadRequest (e);
                }
            }
            Context.SaveChanges ();

            return Ok ();
        }

        [Route ("addplayernickname")]
        [HttpPut]
        public IActionResult AddPlayerNickname ([FromBody] Nickname data) {
            if (data == null) return BadRequest ();
            Context.Add<Nickname> (data);
            Context.SaveChanges ();

            return Ok ();
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
        #endregion

        #region Aux Functions

        /// <summary>
        /// Returns an array of all Players saved in the database.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Player[] GetAllPlayers (PlayerContext context) {
            Player[] result = null;

            result = (from p in context.Players.AsNoTracking () select p).ToArray ();

            return result;
        }
        public static Player GetPlayerWithId (int id, PlayerContext context) {
            Player result = null;

            result = (from p in context.Players where p.PlayerId == id select p).FirstOrDefault ();

            return result;
        }

        public static Player GetPlayerWithName (string name, PlayerContext context) {
            Player result = null;
            result = (from p in context.Players where p.Name == name select p).FirstOrDefault ();
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
        #endregion
    }
}