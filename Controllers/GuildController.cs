using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> GetGuilds () {
            var result = await GetAllGuilds (Context, true);

            if (result == null) {
                return NotFound ();
            }
            return Ok (result);

        }

        [HttpPut]
        public async Task<IActionResult> UpdateGuild (Guild guild) {
            var updated = await UpdateGuild (Context, guild.Id, guild.Name, guild.CurrentMatches);
            if (updated == null) {
                return BadRequest ("No guild to updated");
            }

            return Ok (updated);
        }

        public static async Task<Guild> UpdateGuildMatch (PlayerContext context, ulong guildId, string currentMathches) {
            return await UpdateGuild (context, guildId, currentMathches : currentMathches);
        }

        public static async Task<Guild> UpdateGuild (PlayerContext context, ulong guildId, string name = null, string currentMathches = null) {
            var guild = await GetGuildWithId (context, guildId, false);
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
            await context.SaveChangesAsync ();
            return guild;
        }

        public static async Task<string> GetGuildEvent (PlayerContext context, ulong guildId) {

            var guild = await CreateGuildQuery (context, true).FirstOrDefaultAsync (g => g.Id == guildId);
            return guild?.CurrentMatches ?? "general";
        }

        public static async Task AddGuild (PlayerContext context, ulong guildId, string name) {
            var guild = new Guild () { Id = guildId, Name = name, CurrentMatches = "general" };
            context.Guilds.Add (guild);
            await context.SaveChangesAsync ();
        }

        public static async Task<Guild> GetGuildWithId (PlayerContext context, ulong guildId, bool isReadonly = false) {
            var query = CreateGuildQuery (context, isReadonly);
            return await (from q in query where q.Id == guildId select q).FirstOrDefaultAsync ();
        }

        public static async Task<ICollection<Player>> GetPlayersInGuild (PlayerContext context, ulong guildId) {
            var query = CreateGuildPlayerQuery (context);
            return await (from gp in query where gp.GuildId == guildId select gp.Player).ToListAsync ();
        }

        public static async Task<Guild[]> GetAllGuilds (PlayerContext context, bool isReadonly) {
            return await CreateGuildQuery (context, isReadonly).ToArrayAsync ();
        }

        public static async Task AddGuildToPlayer (PlayerContext context, Guild guild, Player player) {
            var guildPlayer = new GuildPlayer {
                PlayerId = player.PlayerId,
                GuildId = guild.Id,
                Player = player,
                Guild = guild
            };
            player.GuildPlayers.Add (guildPlayer);
            context.GuildPlayers.Add (guildPlayer);
            context.Update<Player> (player);
            await context.SaveChangesAsync ();

        }

        private static IQueryable<Guild> CreateGuildQuery (PlayerContext context, bool isReadonly) {
            return isReadonly ? context.Guilds.AsNoTracking () : context.Guilds;
        }

        private static IQueryable<GuildPlayer> CreateGuildPlayerQuery (PlayerContext context,
            bool includeGuild = false, bool includePlayer = false, bool isReadonly = false) {
            var query = isReadonly ? context.GuildPlayers.AsNoTracking () : context.GuildPlayers;
            if (includeGuild) {
                query.Include (gp => gp.Guild);
            }
            if (includePlayer) {
                query.Include (gp => gp.Player);
            }
            return query;
        }
    }
}