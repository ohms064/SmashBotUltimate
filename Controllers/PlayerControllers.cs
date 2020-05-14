using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            Player[] result = GetAllPlayers (Context, true, true, true, true);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [HttpGet ("{id}")]
        public IActionResult GetPlayer (ulong id) {

            Player result = GetPlayerWithId (id, Context, true, true, true, true);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [Route ("name")]
        [HttpGet ("{name}")]
        public IActionResult GetPlayerByName (string name) {

            Player result = GetPlayerWithName (name, Context, true, true, true, true);
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
                }
                else {
                    return BadRequest ("Received player already exists but no new guild was provided or guild already exists");
                }
            }
            else {
                try {
                    Context.Add<Player> (data);
                }
                catch (Exception e) {
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
        public static void AddPlayer (PlayerContext context, Player player) {
            context.Add<Player> (player);
            context.SaveChanges ();
        }

        public static void AddGuild (PlayerContext context, ulong playerId, Guild guild) {
            var player = GetPlayerWithId (playerId, context, includeGuildPlayer : true);
            AddGuild (context, player, guild);
        }

        public static void AddGuild (PlayerContext context, Player player, Guild guild) {
            var playerGuild = new GuildPlayer { Player = player, PlayerId = player.PlayerId, GuildId = guild.Id, Guild = guild };
            player.GuildPlayers.Append (playerGuild);
            context.GuildPlayers.Add (playerGuild);
            context.Players.Update (player);
            context.SaveChanges ();
        }

        /// <summary>
        /// Returns an array of all Players saved in the database.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Player[] GetAllPlayers (PlayerContext context,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false, bool includeGuildPlayer = false, bool readOnly = true) {

            Player[] result = null;

            var query = CreatePlayerEntities (context, includeMatches, includeOpponentMatches, includeNicknames, includeGuildPlayer, readOnly);

            result = (from p in query select p).ToArray ();

            return result;
        }
        public static Player GetPlayerWithId (ulong id, PlayerContext context,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false, bool includeGuildPlayer = false, bool readOnly = true) {
            Player result = null;

            var query = CreatePlayerEntities (context, includeMatches, includeOpponentMatches, includeNicknames, includeGuildPlayer, readOnly);

            result = (from p in query where p.PlayerId == id select p).FirstOrDefault ();

            return result;
        }

        public static Player GetPlayerWithName (string name, PlayerContext context,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false, bool includeGuildPlayer = false, bool readOnly = true) {

            Player result = null;
            var query = CreatePlayerEntities (context, includeMatches, includeOpponentMatches, includeNicknames, includeGuildPlayer, readOnly);

            result = (from p in query where p.Name == name select p).FirstOrDefault ();
            return result;
        }

        private static IQueryable<Player> CreatePlayerEntities (PlayerContext context, bool includeMatches,
            bool includeOpponentMatches, bool includeNicknames, bool includeGuildPlayer, bool readOnly) {
            var query = readOnly ? context.Players.AsNoTracking () : context.Players;
            if (includeMatches) {
                query = query.Include (p => p.PlayerMatches);
            }
            if (includeOpponentMatches) {
                query = query.Include (p => p.OpponentMatches);
            }
            if (includeNicknames) {
                query = query.Include (p => p.Nicknames);
            }
            if (includeGuildPlayer) {
                query = query.Include (p => p.GuildPlayers);
            }
            return query;
        }
        #endregion
    }
}