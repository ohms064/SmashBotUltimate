using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Modules.DBContextService;

namespace SmashBotUltimate.Bot.Commands {

    [Group ("smashfest")]
    public class SmashfestCommands : BaseCommandModule, IMatchmaking {
        public PlayerDBService DBContext { get; set; }
        public const string Fiend = "fiend";
        public const string Defender = "defender";
        public const string Champion = "champion";
        public const string Master = "master";

        public const string Team = "team";
        public const string Smashfest = "smashfest";

        [Command ("start")]
        [Hidden]
        [RequireRoles (RoleCheckMode.All, "smashfest-admin")]
        public async Task CreateSmashfest (CommandContext context, string teamA, string teamB) {

            if (SmashfestInProgress (context)) {
                await context.RespondAsync ("Ya hay un smashfest en curso");
                return;
            }

            if (teamA.Equals (teamB)) {
                await context.RespondWithFileAsync ("Los equipos tienen el mismo nombre!");
            }

            await context.ReplyAsync ("Creating roles");
            var smashfestRole = await context.Guild.CreateRoleAsync (Smashfest, Permissions.None);

            var teamARole = await context.Guild.CreateRoleAsync ($"{Team}-{teamA}", Permissions.None);
            var teamBRole = await context.Guild.CreateRoleAsync ($"{Team}-{teamB}", Permissions.None);

            var teamAFiend = await context.Guild.CreateRoleAsync ($"{teamA}-{Fiend}", Permissions.None);
            var teamBFiend = await context.Guild.CreateRoleAsync ($"{teamB}-{Fiend}", Permissions.None);

            var teamADefender = await context.Guild.CreateRoleAsync ($"{teamA}-{Defender}", Permissions.None);
            var teamBDefender = await context.Guild.CreateRoleAsync ($"{teamB}-{Defender}", Permissions.None);

            var teamAChampion = await context.Guild.CreateRoleAsync ($"{teamA}-{Champion}", Permissions.None);
            var teamBChampion = await context.Guild.CreateRoleAsync ($"{teamB}-{Champion}", Permissions.None);

            var teamAMaster = await context.Guild.CreateRoleAsync ($"{teamA}-{Master}", Permissions.None);
            var teamBMaster = await context.Guild.CreateRoleAsync ($"{teamB}-{Master}", Permissions.None);

            await context.RespondAsync ("Creating channels");

            var teamAPermissions = CreatePermissions (context.Guild, teamARole, teamBRole);
            var teamBPermissions = CreatePermissions (context.Guild, teamBRole, teamARole);
            var categoryPermissions = CreatePermissions (context.Guild, smashfestRole, context.Guild.EveryoneRole);

            var category = await context.Guild.CreateChannelCategoryAsync (Smashfest);

            var channelA = await context.Guild.CreateTextChannelAsync ($"{Team}-{teamA}", category, overwrites : teamAPermissions);
            var channelB = await context.Guild.CreateTextChannelAsync ($"{Team}-{teamB}", category, overwrites : teamBPermissions);

            await DBContext.UpdateGuildCurrentMatch (context.Guild.Id, $"{Smashfest}_{teamA}_{teamB}");

            await context.RespondAsync ("Finished!");
        }

        [Command ("delete")]
        [RequireRoles (RoleCheckMode.All, "smashfest-admin")]
        [Hidden]
        public async Task DeleteSmashfest (CommandContext context) {
            if (!SmashfestInProgress (context)) {
                await context.RespondAsync ("No hay un smashfest en curso");
                return;
            }

            await context.ReplyAsync ("Deleting roles");

            var rolesToDelete = from r in context.Guild.Roles where RoleIsFromSmashfest (r.Value) select r.Value;

            foreach (var role in rolesToDelete) {
                await role.DeleteAsync ();
            }

            await context.RespondAsync ("Deleting channels");

            var channelsToDelete = from c in context.Guild.Channels where ChannelIsFromSmashfest (c.Value) select c.Value;

            foreach (var channel in channelsToDelete) {
                await channel.DeleteAsync ();
            }

            await DBContext.ResetGuildCurrentMatch (context.Guild.Id);

            await context.RespondAsync ("Finished!");
        }

        [Command ("join")][Aliases ("unir", "entrar")]
        public async Task JoinTeam (CommandContext context, string role) {
            if (!SmashfestInProgress (context)) {
                await context.RespondAsync ("No hay un smashfest en curso");
                return;
            }

            var cleanRole = role.Trim ();
            //Check if user has already joined a smashfest team
            var smashfestRolesCount = (from r in context.Member.Roles where RoleIsFromSmashfest (r) select r).Count ();
            if (smashfestRolesCount > 0) {
                await context.RespondAsync ("Ya te has unido a un equipo de smashfest!");
            }
            var teamRoleRanked = (from r in context.Guild.Roles where r.Value.Name.Equals ($"{role}-{Fiend}") select r.Value).FirstOrDefault ();
            var smashfestRole = (from r in context.Guild.Roles where r.Value.Name.Equals (Smashfest) select r.Value).FirstOrDefault ();
            var teamRole = (from r in context.Guild.Roles where r.Value.Name.Equals ($"{Team}-{role}") select r.Value).FirstOrDefault ();
            if (teamRoleRanked == null || smashfestRole == null || teamRole == null) {
                return;
            }
            await context.Member.GrantRoleAsync (teamRoleRanked);
            await context.Member.GrantRoleAsync (smashfestRole);
            await context.Member.GrantRoleAsync (teamRole);
        }

        private bool RoleIsFromSmashfest (DiscordRole role) {
            return RoleIsRanked (role) || role.Name.StartsWith (Team) || role.Name.Equals (Smashfest);
        }

        private bool RoleIsRanked (DiscordRole role) {
            return role.Name.EndsWith (Fiend) || role.Name.EndsWith (Defender) || role.Name.EndsWith (Champion) ||
                role.Name.EndsWith (Master);
        }

        private bool ChannelIsFromSmashfest (DiscordChannel channel) {
            return channel.Name.StartsWith (Team) || channel.Name.StartsWith (Smashfest);
        }
        private List<DiscordOverwriteBuilder> CreatePermissions (DiscordGuild guild, DiscordRole allowedRole, DiscordRole deniedRole) {
            var permissions = new List<DiscordOverwriteBuilder> ();

            var adminRole = guild.Roles.Values.Where (role => role.Name.Equals ("smashfest-admin")).FirstOrDefault ();
            if (adminRole != null) {
                var adminPermissions = new DiscordOverwriteBuilder ();
                adminPermissions.For (adminRole);
                adminPermissions.Allow (Permissions.SendMessages);
                adminPermissions.Allow (Permissions.AccessChannels);
                adminPermissions.Allow (Permissions.PrioritySpeaker);
                permissions.Add (adminPermissions);
            }
            Console.WriteLine ($"allowed: {allowedRole.Name}");
            DiscordOverwriteBuilder allowed = new DiscordOverwriteBuilder ();
            allowed.For (allowedRole);
            allowed.Allow (Permissions.SendMessages);
            allowed.Allow (Permissions.AccessChannels);
            allowed.Allow (Permissions.ReadMessageHistory);
            permissions.Add (allowed);

            Console.WriteLine ($"denied: {deniedRole.Name}");
            DiscordOverwriteBuilder denied = new DiscordOverwriteBuilder ();
            denied.For (deniedRole);
            denied.Deny (Permissions.SendMessages);
            denied.Deny (Permissions.AccessChannels);
            denied.Deny (Permissions.ReadMessageHistory);
            permissions.Add (denied);

            return permissions;
        }

        private bool SmashfestInProgress (CommandContext context) {
            var channels = from c in context.Guild.Channels where ChannelIsFromSmashfest (c.Value) select c.Value;
            return channels.Count () > 0;
        }

        public string GetKey () {
            return Smashfest;
        }

        public async Task<bool> MatchmakingFilter (string topic, DiscordMember challenger, DiscordMember opponentCandidate) {
            var matches = await DBContext.GetMatches (challenger.Guild.Id, challenger, opponentCandidate);
            if (!matches[0].PendingFight || !matches[1].PendingFight) return false;
            var challengerTeam = UserTeam (challenger, topic);
            var opponentTeam = UserTeam (opponentCandidate, topic);
            if (challengerTeam < 0 || opponentTeam < 0 || challengerTeam == opponentTeam) return false;
            return true;
        }

        private short UserTeam (DiscordMember member, string smashfestTopic) {
            if (!IsSmashfestTopic (smashfestTopic)) return -1; //Checks if we have an ongoing smashfest

            var smashfestTeams = smashfestTopic.Split ("_");

            var role = member.Roles.FirstOrDefault (r => RoleIsRanked (r));
            if (role == null) return -1;

            if (role.Name.StartsWith (smashfestTeams[1])) {
                return 1;
            }
            if (role.Name.StartsWith (smashfestTeams[2])) {
                return 2;
            }
            return -1;
        }

        private bool IsSmashfestTopic (string topic) {
            return topic.StartsWith (Smashfest);
        }
    }
}