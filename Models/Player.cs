using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Sqlite;
//References : https://stackoverflow.com/questions/39728016/self-referencing-many-to-many-relations

namespace SmashBotUltimate.Models {

    public class PlayerContext : DbContext {
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        //public DbSet<PlayerMatch> PlayerMatches { get; set; }
        public DbSet<Nickname> PlayerNicknames { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<GuildPlayer> GuildPlayers { get; set; }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite ("Data Source=inscripcion.db");
        }

        protected override void OnModelCreating (ModelBuilder builder) {

            builder.Entity<Player> ().HasKey (p => p.PlayerId);
            builder.Entity<Match> ().HasKey (c => c.Id);
            //builder.Entity<PlayerMatch> ().HasKey (c => new { c.MatchId, c.PlayerId });
            builder.Entity<Nickname> ().HasKey (p => p.NicknameId);
            builder.Entity<Guild> ().HasKey (propertyNames => propertyNames.Id);
            builder.Entity<GuildPlayer> ().HasKey (propertyNames => new { propertyNames.GuildId, propertyNames.PlayerId });

            builder.Entity<Player> ().Property (p => p.PlayerId).ValueGeneratedOnAdd ();
            builder.Entity<Match> ().Property (p => p.Id).ValueGeneratedOnAdd ();

            builder.Entity<Player> ()
                .HasIndex (p => p.Name)
                .IsUnique ();

            builder.Entity<GuildPlayer> ()
                .HasOne (gp => gp.Player)
                .WithMany (player => player.GuildPlayers)
                .HasForeignKey (gp => gp.PlayerId);

            builder.Entity<GuildPlayer> ()
                .HasOne (gp => gp.Guild)
                .WithMany (guild => guild.GuildPlayers)
                .HasForeignKey (gp => gp.GuildId);
            /*
                        builder.Entity<PlayerMatch> ()
                            .HasOne (p => p.Player)
                            .WithMany (p => p.PlayerMatches)
                            .HasForeignKey (p => p.PlayerId);

                        builder.Entity<PlayerMatch> ()
                            .HasOne (p => p.Match)
                            .WithOne (p => p.PlayerMatch);
            ¨*/
            builder.Entity<Match> ()
                .HasOne (m => m.OpponentPlayer)
                .WithMany (p => p.OpponentMatches)
                .HasForeignKey (p => p.OpponentPlayerId);

            builder.Entity<Player> ()
                .HasMany (p => p.PlayerMatches);

            builder.Entity<Nickname> ()
                .HasOne (p => p.OriginPlayer)
                .WithMany (p => p.Nicknames)
                .HasForeignKey (p => p.PlayerId);

            builder.Entity<Player> ()
                .HasMany (p => p.Nicknames);

        }
    }

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

    public class Nickname {

        public int NicknameId { get; set; }
        public string Platform { get; set; }
        public string Name { get; set; }

        public int PlatformId { get; set; }

        public ulong PlayerId { get; set; }

        [JsonIgnore]
        public virtual Player OriginPlayer { get; set; }
    }

    public class Match {
        public const string DefaultTopic = "general";
        public int Id { get; set; }

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

    public class Guild {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public ICollection<GuildPlayer> GuildPlayers { get; set; }
        public string CurrentMatches { get; set; }
    }

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