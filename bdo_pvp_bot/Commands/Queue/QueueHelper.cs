using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Domain.Entities;
using Serilog;
using System.Text.RegularExpressions;

namespace bdo_pvp_bot.Commands.Queue
{
    public class QueueHelper
    {
        public async Task<List<RestTextChannel>> CreateMatchAsync(List<User> team1, List<User> team2, SocketGuild guild, SolareService solareService)
        {
            try
            {
                var match = await solareService.CreateMatchAsync(team1, team2);
                var category = await guild.CreateCategoryChannelAsync($"MatchId-{match.Id}");

                var everyoneRole = guild.EveryoneRole;
                var denyEveryone = new OverwritePermissions(viewChannel: PermValue.Deny);

                var team1Permissions = team1.ToDictionary(
                    user => guild.GetUser(user.DiscordId).Id,
                    user => new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow, speak: PermValue.Allow)
                );

                var team2Permissions = team2.ToDictionary(
                    user => guild.GetUser(user.DiscordId).Id,
                    user => new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow, speak: PermValue.Allow)
                );

                var resultChannel = await guild.CreateTextChannelAsync("Результат", x =>
                {
                    x.CategoryId = category.Id;
                    x.PermissionOverwrites = new List<Overwrite>
            {
                new Overwrite(everyoneRole.Id, PermissionTarget.Role, denyEveryone),
                new Overwrite(guild.GetUser(team1[0].DiscordId).Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny, addReactions: PermValue.Allow)),
                new Overwrite(guild.GetUser(team2[0].DiscordId).Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny, addReactions: PermValue.Allow))
            };
                });

                var alarmChannel = await guild.CreateTextChannelAsync("Уведомление", x =>
                {
                    x.CategoryId = category.Id;
                    var overwrites = new List<Overwrite> { new Overwrite(everyoneRole.Id, PermissionTarget.Role, denyEveryone) };
                    overwrites.AddRange(team1.Select(user => new Overwrite(guild.GetUser(user.DiscordId).Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny, addReactions: PermValue.Deny))));
                    overwrites.AddRange(team2.Select(user => new Overwrite(guild.GetUser(user.DiscordId).Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny, addReactions: PermValue.Deny))));
                    x.PermissionOverwrites = overwrites;
                });

                // Создаем голосовые каналы с нужными правами доступа
                var teamChannel1 = await guild.CreateVoiceChannelAsync("Команда 1", x =>
                {
                    x.CategoryId = category.Id;
                    var overwrites = new List<Overwrite> { new Overwrite(everyoneRole.Id, PermissionTarget.Role, denyEveryone) };
                    overwrites.AddRange(team1.Select(user => new Overwrite(guild.GetUser(user.DiscordId).Id, PermissionTarget.User, team1Permissions[user.DiscordId])));
                    overwrites.AddRange(team2.Select(user => new Overwrite(guild.GetUser(user.DiscordId).Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Deny, speak: PermValue.Deny))));
                    x.PermissionOverwrites = overwrites;
                });

                var teamChannel2 = await guild.CreateVoiceChannelAsync("Команда 2", x =>
                {
                    x.CategoryId = category.Id;
                    var overwrites = new List<Overwrite> { new Overwrite(everyoneRole.Id, PermissionTarget.Role, denyEveryone) };
                    overwrites.AddRange(team1.Select(user => new Overwrite(guild.GetUser(user.DiscordId).Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Deny, speak: PermValue.Deny))));
                    overwrites.AddRange(team2.Select(user => new Overwrite(guild.GetUser(user.DiscordId).Id, PermissionTarget.User, team2Permissions[user.DiscordId])));
                    x.PermissionOverwrites = overwrites;
                });
                var channels = new List<RestTextChannel> { alarmChannel, resultChannel };
                return channels;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка создания матча");
                return null;
            }
        }
    }
}
