namespace Backend.Models {
    public class TwoIds {
        public ulong WinnerId { get; set; }
        public ulong LoserId { get; set; }
        public string Topic { get; set; }

        public ulong GuildId { get; set; }
    }

    public class OneId {
        public ulong PlayerId { get; set; }
    }
}