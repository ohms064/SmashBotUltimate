using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
namespace SmashBotUltimate.Models {
    public class GuildPlayer {
        public ulong PlayerId { get; set; }
        public ulong GuildId { get; set; }

        [JsonIgnore]
        public Player Player { get; set; }

        [JsonIgnore]
        public Guild Guild { get; set; }
    }
    /* 
        public class PlayerMatch {
            public int PlayerId { get; set; }
            public int MatchId { get; set; }

            public Player Player { get; set; }
            public Match Match { get; set; }
        }
        */
}