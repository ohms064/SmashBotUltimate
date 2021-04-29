using System;
namespace SmashBotUltimate.Models {
    public class Lobby {
        public const int HourLimit = 4;
        public ulong GuildId { get; set; }

        public bool Global { get; set; }
        public ulong ChannelId { get; set; }
        public string RoomId { get; set; }
        public string Password { get; set; }
        public bool HasComment { get => !string.IsNullOrEmpty (Comment); }
        public bool HasPassword { get => !string.IsNullOrEmpty (Password); }
        public string Comment { get; set; }
        public ulong OwnerId { get; set; }
        public DateTimeOffset PublishTime { get => _publishTime; set { _publishTime = value; RemovalReferenceTime = value.AddHours (HourLimit); } }
        public DateTimeOffset RemovalReferenceTime { get; set; }

        private DateTimeOffset _publishTime;

        public TimeSpan Duration (DateTimeOffset other) {
            return other - PublishTime;
        }
    }
}