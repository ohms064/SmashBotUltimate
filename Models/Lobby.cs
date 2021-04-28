using System;
namespace SmashBotUltimate.Models {
    public class Lobby {
        public ulong GuildId { get; set; }

        public bool Global { get; set; }
        public ulong ChannelId { get; set; }
        public string RoomId { get; set; }
        public string Password { get; set; }
        public ulong OwnerId { get; set; }
        public DateTimeOffset PublishTime { get; set; }
        public DateTimeOffset RemovalReferenceTime { get; set; }

        public TimeSpan Duration (DateTimeOffset other) {
            return other - PublishTime;
        }
    }
}