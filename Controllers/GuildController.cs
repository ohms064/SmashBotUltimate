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
        public static string GetGuildEvent (PlayerContext context, ulong guildId) {
            var guild = context.Guilds.AsNoTracking ().Where (g => g.Id == guildId).FirstOrDefault ();
            return guild?.CurrentMatches ?? null;
        }

        public static Guild GetGuildWithId (PlayerContext context, ulong guildId, bool isReadonly = false) {
            var query = CreateQuery (context, isReadonly);
            return (from q in query where q.Id == guildId select q).FirstOrDefault ();
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

        private static IQueryable<Guild> CreateQuery (PlayerContext context, bool isReadonly) {
            return isReadonly ? context.Guilds.AsNoTracking () : context.Guilds;
        }
    }
}