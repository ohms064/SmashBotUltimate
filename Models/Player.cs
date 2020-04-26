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
        public DbSet<PlayerScore> PlayerScore { get; set; }
        public DbSet<PlayerNickname> PlayerNickname { get; set; }
        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite ("Data Source=inscripcion.db");
        }

        protected override void OnModelCreating (ModelBuilder builder) {
            builder.Entity<Player> ().HasKey (p => p.PlayerId);
            builder.Entity<PlayerScore> ().HasKey (c => new { c.Player1Id, c.Player2Id });
            builder.Entity<PlayerNickname> ().HasKey (p => p.NicknameId);

            builder.Entity<Player> ().Property (p => p.PlayerId).ValueGeneratedOnAdd ();
            builder.Entity<PlayerScore> ()
                .HasOne (p => p.Player1)
                .WithMany (p => p.PlayerMatches)
                .HasForeignKey (p => p.Player1Id)
                .IsRequired ();

            builder.Entity<PlayerScore> ()
                .HasOne (p => p.Player2)
                .WithMany (p => p.PlayerMatches)
                .HasForeignKey (p => p.Player2Id)
                .IsRequired ();

            //builder.Entity<
            builder.Entity<Player> ()
                .HasMany (p => p.PlayerMatches);

            builder.Entity<PlayerNickname> ()
                .HasOne (p => p.Player)
                .WithMany (p => p.Nickname)
                .HasForeignKey (p => p.PlayerId);

            builder.Entity<Player> ().
            HasMany (p => p.Nickname);
        }
    }

    public class Player {

        public int PlayerId { get; set; }

        public string Name { get; set; }

        public int Nivel { get; set; }
        public virtual ICollection<PlayerScore> PlayerMatches { get; set; }

        public virtual ICollection<PlayerNickname> Nickname { get; set; }
    }

    public class PlayerNickname {

        public int NicknameId { get; set; }
        public string Platform { get; set; }
        public string Nickname { get; set; }

        public int PlayerId { get; set; }

        public virtual Player Player { get; set; }
    }

    public class PlayerScore {
        public int Player1Id {
            get;
            set;
        }

        public int Player2Id {
            get;
            set;
        }

        public virtual Player Player1 { get; set; }

        public virtual Player Player2 { get; set; }

        public bool PendingFight { get; set; }

        public int Player1WinCount { get; set; }
        public int Player2WinCount { get; set; }

        public DateTime LastMatch { get; set; }

        public int Matches { get; set; }
    }
}