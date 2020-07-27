using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
//References : https://stackoverflow.com/questions/39728016/self-referencing-many-to-many-relations

namespace SmashBotUltimate.Models {

    public class Player {

        public ulong PlayerId { get; set; }
        //Unique, should be Discord username#discriminator
        public string Name { get; set; }

        public int Nivel { get; set; }
        public virtual ICollection<Match> PlayerMatches { get; set; }

        public virtual ICollection<Match> OpponentMatches { get; set; }

        public virtual ICollection<Nickname> Nicknames { get; set; }

        public virtual ICollection<GuildPlayer> GuildPlayers { get; set; }

        public bool HasGuildId (ulong id) {
            foreach (var g in GuildPlayers) {
                if (g.GuildId == id)
                    return true;
            }
            return false;
        }
    }

}