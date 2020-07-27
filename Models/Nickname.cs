using System.Text.Json.Serialization;

namespace SmashBotUltimate.Models {
    public class Nickname {

        public int NicknameId { get; set; }
        public string Platform { get; set; }
        public string Name { get; set; }

        public int PlatformId { get; set; }

        public ulong PlayerId { get; set; }

        [JsonIgnore]
        public virtual Player OriginPlayer { get; set; }
    }

}