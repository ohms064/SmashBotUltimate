namespace Backend.Models {
    public class TwoIds {
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
        public string Topic { get; set; }
    }

    public class OneId {
        public int PlayerId { get; set; }
    }
}