using System.Threading.Tasks;
using DSharpPlus.Entities;
public interface IMatchmaking {
    string GetKey ();
    Task<bool> MatchmakingFilter (string topic, DiscordMember challenger, DiscordMember opponent);
}