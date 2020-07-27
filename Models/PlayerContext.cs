using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace SmashBotUltimate.Models {
    public class PlayerContext : DbContext {
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        //public DbSet<PlayerMatch> PlayerMatches { get; set; }
        public DbSet<Nickname> PlayerNicknames { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<GuildPlayer> GuildPlayers { get; set; }

        public DbSet<Lobby> Lobbies { get; set; }

        public PlayerContext (DbContextOptions<PlayerContext> options) : base (options) { }

        protected override void OnModelCreating (ModelBuilder builder) {

            builder.Entity<Player> ().HasKey (p => p.PlayerId);
            builder.Entity<Match> ().HasKey (c => c.Id);
            //builder.Entity<PlayerMatch> ().HasKey (c => new { c.MatchId, c.PlayerId });
            builder.Entity<Nickname> ().HasKey (p => p.NicknameId);
            builder.Entity<Guild> ().HasKey (propertyNames => propertyNames.Id);
            builder.Entity<GuildPlayer> ().HasKey (propertyNames => new { propertyNames.GuildId, propertyNames.PlayerId });
            builder.Entity<Lobby> ().HasKey (l => new { l.GuildId, l.ChannelId, l.OwnerId });

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

}