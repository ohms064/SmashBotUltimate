using System;
namespace SmashBotUltimate.Models {
    public class Lobby {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string RoomId { get; set; }
        public string Password { get; set; }
        public bool HasComment { get => !string.IsNullOrEmpty (Comment); }
        public string Comment { get; set; }
        public ulong OwnerId { get; set; }
        public DateTimeOffset PublishTime { get; set; }

        public TimeSpan Duration (DateTimeOffset other) {
            return other - PublishTime;
        }
    }
}