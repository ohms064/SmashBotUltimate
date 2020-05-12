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
    public class GuildController : ControllerBase {

        public PlayerContext Context { get; set; }

        public GuildController (PlayerContext context) {
            Context = context;
        }

        [HttpGet ()]
        public IActionResult GetGuilds () {
            var result = GetAllGuilds (Context, true);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [HttpPut]
        public IActionResult UpdateGuild (Guild guild) {
            var updated = UpdateGuild (Context, guild.Id, guild.Name, guild.CurrentMatches);
            if (updated == null) {
                return BadRequest ("No guild to updated");
            }

            return Ok (updated);
        }

        public static Guild UpdateGuild (PlayerContext context, ulong guildId, string name = null, string currentMathches = null) {
            var guild = GetGuildWithId (context, guildId, false);
            if (guild == null) {
                return null;
            }
            if (!string.IsNullOrEmpty (name) && !name.Equals (guild.Name)) {
                guild.Name = name;
            }

            if (!string.IsNullOrEmpty (currentMathches) && !currentMathches.Equals (guild.CurrentMatches)) {
                guild.CurrentMatches = currentMathches;
            }

            context.Guilds.Update (guild);
            context.SaveChanges ();
            return guild;
        }

        public static string GetGuildEvent (PlayerContext context, ulong guildId) {

            var guild = (from g in CreateGuildQuery (context, true) where g.Id == guildId select g).FirstOrDefault ();
            return guild?.CurrentMatches ?? null;
        }

        public static void AddGuild (PlayerContext context, ulong guildId, string name) {
            var guild = new Guild () { Id = guildId, Name = name, CurrentMatches = "general" };
            context.Guilds.Add (guild);
            context.SaveChanges ();
        }

        public static Guild GetGuildWithId (PlayerContext context, ulong guildId, bool isReadonly = false) {
            var query = CreateGuildQuery (context, isReadonly);
            return (from q in query where q.Id == guildId select q).FirstOrDefault ();
        }

        public static Guild[] GetAllGuilds (PlayerContext context, bool isReadonly) {
            return CreateGuildQuery (context, isReadonly).ToArray ();
        }

        public static void AddGuildToPlayer (PlayerContext context, ref Guild guild, ref Player player) {
            var guildPlayer = new GuildPlayer {
                PlayerId = player.PlayerId,
                GuildId = guild.Id,
                Player = player,
                Guild = guild
            };
            player.GuildPlayers.Add (guildPlayer);
            context.GuildPlayers.Add (guildPlayer);
            context.Update<Player> (player);
            context.SaveChanges ();

        }

        private static IQueryable<Guild> CreateGuildQuery (PlayerContext context, bool isReadonly) {
            return isReadonly ? context.Guilds.AsNoTracking () : context.Guilds;
        }
    }
}