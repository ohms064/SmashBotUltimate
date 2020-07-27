using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
namespace SmashBotUltimate.Models {
    public class Match {
        public const string DefaultTopic = "general";
        public int Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong OpponentPlayerId { get; set; }

        [JsonIgnore]
        public Player OpponentPlayer { get; set; }

        //public int PlayerMatchId { get; set; }

        //public virtual PlayerMatch PlayerMatch { get; set; }

        public bool PendingFight { get; set; }

        public int WinCount { get; set; }

        public DateTime LastMatch { get; set; }

        public TimeSpan Interval { get; set; }

        //Could be a tourney or in general.
        public string Topic { get; set; }
    }

}