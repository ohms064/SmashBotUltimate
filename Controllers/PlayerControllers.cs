using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<IActionResult> GetPlayers () {
            Player[] result = await GetAllPlayers (Context, true, true, true, true);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [HttpGet ("{id}")]
        public async Task<IActionResult> GetPlayer (ulong id) {

            Player result = await GetPlayerWithId (id, Context, true, true, true, true);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [Route ("name")]
        [HttpGet ("{name}")]
        public async Task<IActionResult> GetPlayerByName (string name) {

            Player result = await GetPlayerWithName (name, Context, true, true, true, true);
            if (result == null) {
                return NotFound ();
            }
            return Ok ();

        }
        #endregion

        #region HttpPost
        [Route ("addplayer")]
        [HttpPost]
        public async Task<IActionResult> AddPlayer ([FromBody] Player data) {
            if (data == null) return BadRequest ();

            var existingPlayer = await GetPlayerWithName (data.Name, Context);

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
        public async Task<IActionResult> AddPlayerNickname ([FromBody] Nickname data) {
            if (data == null) return BadRequest ();
            await Context.AddAsync<Nickname> (data);
            await Context.SaveChangesAsync ();

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
        public async static Task AddPlayer (PlayerContext context, Player player) {
            await context.AddAsync<Player> (player);
            await context.SaveChangesAsync ();
        }

        public async static Task AddGuild (PlayerContext context, ulong playerId, Guild guild) {
            var player = await GetPlayerWithId (playerId, context, includeGuildPlayer : true);
            await AddGuild (context, player, guild);
        }

        public async static Task AddGuild (PlayerContext context, Player player, Guild guild) {
            var playerGuild = new GuildPlayer { Player = player, PlayerId = player.PlayerId, GuildId = guild.Id, Guild = guild };
            player.GuildPlayers.Append (playerGuild);
            await context.GuildPlayers.AddAsync (playerGuild);
            context.Players.Update (player);
            await context.SaveChangesAsync ();
        }

        /// <summary>
        /// Returns an array of all Players saved in the database.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async static Task<Player[]> GetAllPlayers (PlayerContext context,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false, bool includeGuildPlayer = false, bool readOnly = true) {

            Player[] result = null;

            var query = CreatePlayerEntities (context, includeMatches, includeOpponentMatches, includeNicknames, includeGuildPlayer, readOnly);

            result = await (from p in query select p).ToArrayAsync ();

            return result;
        }
        public async static Task<Player> GetPlayerWithId (ulong id, PlayerContext context,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false, bool includeGuildPlayer = false, bool readOnly = true) {
            Player result = null;

            var query = CreatePlayerEntities (context, includeMatches, includeOpponentMatches, includeNicknames, includeGuildPlayer, readOnly);

            result = await (from p in query where p.PlayerId == id select p).FirstOrDefaultAsync ();

            return result;
        }

        public async static Task<Player> GetPlayerWithName (string name, PlayerContext context,
            bool includeMatches = false, bool includeOpponentMatches = false, bool includeNicknames = false, bool includeGuildPlayer = false, bool readOnly = true) {

            Player result = null;
            var query = CreatePlayerEntities (context, includeMatches, includeOpponentMatches, includeNicknames, includeGuildPlayer, readOnly);

            result = await (from p in query where p.Name == name select p).FirstOrDefaultAsync ();
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