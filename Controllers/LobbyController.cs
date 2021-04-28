using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmashBotUltimate.Models;

namespace SmashBotUltimate.Controllers {
    public class LobbyController : ControllerBase {
        public PlayerContext context { get; set; }
        public LobbyController (PlayerContext context) {
            this.context = context;
        }

        public async static Task<ICollection<Lobby>> GetLobbies (PlayerContext context, DiscordGuild guild, DiscordChannel channel) {
            return await (from l in context.Lobbies where l.GuildId == guild.Id && l.ChannelId == channel.Id select l).ToListAsync ();
        }

        public async static Task<Lobby> GetLobby (PlayerContext context, DiscordGuild guild, DiscordChannel channel, DiscordUser user) {
            return await context.Lobbies.FindAsync (guild.Id, channel.Id, user.Id);
        }

        public async static Task<Lobby> GetLobby (PlayerContext context, ulong guildId, ulong channelId, ulong userId) {
            return await context.Lobbies.FindAsync (guildId, channelId, userId);
        }

        public async static Task<ICollection<Lobby>> GetGlobalLobbies (PlayerContext context) {

            return await (from lobby in context.Lobbies where lobby.Global select lobby).ToListAsync ();
        }
        public async static Task CreateLobby (PlayerContext context, DiscordGuild guild, DiscordChannel channel, DiscordUser user, string lobbyId,
            string password, bool global, DateTimeOffset publishTime) {
            await context.Lobbies.AddAsync (new Lobby () {
                GuildId = guild.Id,
                    ChannelId = channel.Id,
                    OwnerId = user.Id,
                    RoomId = lobbyId,
                    Password = password,
                    PublishTime = publishTime,
                    RemovalReferenceTime = publishTime,
                    Global = global
            });
            await context.SaveChangesAsync ();
        }

        public async static Task CreateLobby (PlayerContext context, Lobby lobby) {
            await context.Lobbies.AddAsync (lobby);
            await context.SaveChangesAsync ();
        }

        public async static Task<Lobby> PopLobby (PlayerContext context, ulong guild, ulong channel, ulong user) {
            var lobby = await GetLobby (context, guild, channel, user);
            if (lobby == null) return null;
            context.Lobbies.Remove (lobby);
            await context.SaveChangesAsync ();
            return lobby;
        }

        public async static Task UpdateLobby (PlayerContext context, Lobby lobby) {
            context.Lobbies.Update (lobby);
            await context.SaveChangesAsync ();
        }

        public static async Task DeleteLobbies (PlayerContext context, params Lobby[] lobby) {
            context.Lobbies.RemoveRange (lobby);
            await context.SaveChangesAsync ();
        }
    }
}