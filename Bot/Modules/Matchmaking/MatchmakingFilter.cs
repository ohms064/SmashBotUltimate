using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Models;

public interface IMatchmakingFilter {
    Task<bool> MatchmakingFilterMember (DiscordGuild guild, Player challenger, Player opponentCandidate);
}
public class MatchmakingFilter : IMatchmakingFilter {

    public delegate Task<bool> Filter (string topic, DiscordMember challenger, DiscordMember opponentCandidate);

    public PlayerDBService DBConnection { get; set; }

    private Dictionary<string, Filter> _matchmakerServices;

    private string _currentTopic = Match.DefaultTopic;

    public MatchmakingFilter (PlayerDBService connection, IEnumerable<IMatchmaking> matchmakings) {
        DBConnection = connection;
        _matchmakerServices = new Dictionary<string, Filter> ();
        foreach (var matchmaker in matchmakings) {
            _matchmakerServices.Add (matchmaker.GetKey (), matchmaker.MatchmakingFilter);
        }
        _matchmakerServices.Add (Match.DefaultTopic, GeneralFilter);
    }

    public async Task<bool> MatchmakingFilterMember (DiscordGuild guild, Player challenger, Player opponentCandidate) {
        var challengerMember = await guild.GetMemberAsync (challenger.PlayerId);
        var opponentMember = await guild.GetMemberAsync (opponentCandidate.PlayerId);
        return await MatchmakingFilterMember (challengerMember, opponentMember);
    }

    private async Task<bool> MatchmakingFilterMember (DiscordMember challenger, DiscordMember opponentCandidate) {
        var topic = await DBConnection.GetGuildCurrentMatch (challenger.Guild.Id);
        var filteredTopic = topic.Split ("_").FirstOrDefault ();
        if (_matchmakerServices.TryGetValue (filteredTopic, out Filter callback)) {
            return await callback.Invoke (topic, challenger, opponentCandidate);
        }
        return true;
    }

    private async Task<bool> GeneralFilter (string topic, DiscordMember challenger, DiscordMember opponent) {
        var matches = await DBConnection.GetMatches (challenger.Guild.Id, challenger, opponent);
        foreach (var match in matches) {
            if (!match.PendingFight) {
                return false;
            }
        }
        return true;
    }
}