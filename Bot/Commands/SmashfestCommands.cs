using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SmashBotUltimate.Bot.Extensions;
using SmashBotUltimate.Bot.Models;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Bot.Modules.InstructionService;

namespace SmashBotUltimate.Bot.Commands {
    public class SmashfestCommands : BaseCommandModule {
        [Command ("smashfest")]
        [RequireOwner]
        [RequireRoles (RoleCheckMode.All, "smashfest-admin")]
        [RequireBotPermissions (Permissions.ManageRoles)]
        public async Task CreateSmashfest (CommandContext context, string teamA, string teamB) {
            await context.Guild.CreateRoleAsync (teamA, Permissions.None);
            await context.Guild.CreateRoleAsync (teamB, Permissions.None);
        }

    }
}