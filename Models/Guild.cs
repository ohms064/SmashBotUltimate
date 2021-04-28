using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace SmashBotUltimate.Models {
    public class Guild {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public ICollection<GuildPlayer> GuildPlayers { get; set; }
        public string CurrentMatches { get; set; }

        public ulong GlobalArenaChannel = 0;
    }

}