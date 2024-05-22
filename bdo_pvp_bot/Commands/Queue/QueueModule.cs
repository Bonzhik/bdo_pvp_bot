using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.Interactions;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Serilog;
using System.Runtime.CompilerServices;

namespace bdo_pvp_bot.Commands.Queue
{
    public class QueueModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SolareService _solareService;
        private readonly UserService _userService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly QueueHelper _queueHelper;

        private static readonly List<List<User>> queue = new List<List<User>>();
        private const int TeamSize = 1;
        private const int MatchSize = TeamSize * 2;
        private bool isAnalyzing = false;
        private bool isAnalyzingRunning = false;
        private int maxEloDifference = 100;
        private const int EloDifferenceIncrement = 10;
        private static Dictionary<ulong, (List<IGuildUser> team, IGuildUser initiator)> PendingConfirmations = new Dictionary<ulong, (List<IGuildUser> team, IGuildUser initiator)>();

        public QueueModule(SolareService solareService, UserService userService, IServiceScopeFactory scopeFactory, QueueHelper queueHelper)
        {
            _solareService = solareService;
            _userService = userService;
            _scopeFactory = scopeFactory;
            _queueHelper = queueHelper;

        }


        [SlashCommand("queue-duo", "Поиск вдвоем")]
        public async Task QueueDuo(IGuildUser user)
        {
            Log.Information($"Попытка создать пати {Context.User.GlobalName}, {user.GlobalName}");

            var initiator = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Player").Id;
            var team = new List<IGuildUser> { initiator, user };

            if (team.Any(u => !u.RoleIds.Contains(roleId)))
            {
                await RespondAsync("Не все корректно добавили персонажа", ephemeral: true);
                return;
            }


            if ( user.Id == initiator.Id || user.IsBot )
            {
                await RespondAsync("Нельзя пригласить себя", ephemeral: true);
                return;
            }

            await RespondAsync("Приглашение отправлены", ephemeral: true);

            await SendConfirmationRequest(team, initiator);
        }

        [SlashCommand("queue-trio", "Поиск втроём")]
        public async Task QueueTrio(IGuildUser user1, IGuildUser user2)
        {
            Log.Information($"Попытка создать пати {Context.User.GlobalName}, {user1.GlobalName}, {user2.GlobalName}");

            var initiator = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Player").Id;
            var team = new List<IGuildUser> { initiator, user1, user2 };

            if (team.Any(u => !u.RoleIds.Contains(roleId)))
            {
                await RespondAsync("Не все корректно добавили персонажа", ephemeral: true);
                return;
            }

            if ( initiator.Id == user1.Id || initiator.Id == user2.Id || user1.Id == user2.Id || user1.IsBot || user2.IsBot)
            {
                await RespondAsync("Команда должна состоять из уникальных игроков", ephemeral: true);
                return;
            }

            await RespondAsync("Приглашение отправлены", ephemeral: true);

            await SendConfirmationRequest(team, initiator);
        }

        private async Task SendConfirmationRequest(List<IGuildUser> team, IGuildUser initiator)
        {
            foreach (var user in team.Skip(1))
            {
                var component = new ComponentBuilder()
                    .WithButton("Принять", $"confirm_{user.Id}", ButtonStyle.Success)
                    .WithButton("Отклонить", $"decline_{user.Id}", ButtonStyle.Danger)
                    .Build();

                await user.SendMessageAsync($"Вы были приглашены в команду {initiator.Username}. Принять приглашение?", components: component);

                PendingConfirmations[user.Id] = (team, initiator);
            }
        }

        [ComponentInteraction("confirm_*")]
        public async Task ConfirmAsync(string userIdStr)
        {
            await DeferAsync();
            var channel = Context.Channel as IDMChannel;
            var userId = ulong.Parse(userIdStr);
            if (PendingConfirmations.TryGetValue(userId, out var info))
            {
                var (team, initiator) = info;
                PendingConfirmations.Remove(userId);

                if (team.All(u => u.Id == initiator.Id || !PendingConfirmations.ContainsKey(u.Id)))
                {
                    List<User> users = new List<User>();
                    foreach(var user in team)
                    {
                        var userTeam = await _userService.FindAsync(user.Id);
                        if (queue.Any(team => team.Any(u => u.DiscordId == user.Id)))
                        {
                            await initiator.SendMessageAsync($"{user.GlobalName} уже находится в очереди. Поиск не будет запущен");
                            await ClearBotMessage(channel);
                            return;
                        }
                        if (userTeam.CurrentCharacter == null)
                        {
                            await initiator.SendMessageAsync($"{user.GlobalName} не выбрал персонажа. Поиск не будет запущен");
                            await ClearBotMessage(channel);
                            return;
                        }
                        if (userTeam.IsInMatch == true)
                        {
                            await initiator.SendMessageAsync($"{user.GlobalName} уже находится в матче. Поиск не будет запущен");
                            await ClearBotMessage(channel);
                            return;
                        }
                        if (users.Any(u => u.CurrentCharacter.Class.Name == userTeam.CurrentCharacter.Class.Name))
                        {
                            await initiator.SendMessageAsync($"В команде не должно быть одинаковых персонажей");
                            await ClearBotMessage(channel);
                            return;
                        }
                        users.Add(userTeam);
                    }
                    try
                    {
                        queue.Add(users);
                        Log.Information($"Команда из {users.Count} человек добавлена в очередь");
                        if (!isAnalyzingRunning)
                        {
                            isAnalyzingRunning = true;
                            _ = Task.Run(AnalyzeQueue);
                        }
                        await initiator.SendMessageAsync($"Команда добавлена в очередь. В очереди {queue.Sum(list => list.Count)} человек");
                    }
                    catch (Exception ex)
                    {
                        Log.Information($"Ошибка добавления команды из {users.Count} человек в очередь");
                    }
                }
            }

            await FollowupAsync("Вы приняли приглашение", ephemeral: true);
            await ClearBotMessage(channel);

        }

        [ComponentInteraction("decline_*")]
        public async Task DeclineAsync(string userIdStr)
        {
            await DeferAsync();
            var channel = Context.Channel as IDMChannel;

            var userId = ulong.Parse(userIdStr);
            if (PendingConfirmations.TryGetValue(userId, out var info))
            {
                var (team, initiator) = info;
                PendingConfirmations.Remove(userId);

                await initiator.SendMessageAsync($"{Context.User.Username} отклонил приглашение. Команда не будет добавлена в очередь.");
            }

            await FollowupAsync("Вы отклонили приглашение", ephemeral: true);
            await ClearBotMessage(channel);

        }

        [SlashCommand("queue-solo", "Соло поиск")]
        public async Task StartQueue()
        {
            var userGuild = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Player").Id;
            var userDb = await _userService.FindAsyncWithoutLazy(userGuild.Id);

            Log.Information($"{userGuild.DisplayName} Попытка начать поиск");

            await DeferAsync(ephemeral: true);

            if (!userGuild.RoleIds.Any(r => r == roleId))
            {
                await FollowupAsync("Нужно добавить персонажа /add-character", ephemeral: true);
                return;
            }
            else if (userDb.CurrentCharacter == null)
            {
                await FollowupAsync("Нужно выбрать персонажа /select-character", ephemeral: true);
                return;
            }
            else if (queue.Any(team => team.Any(u => u.DiscordId == userGuild.Id)))
            {
                await FollowupAsync("Вы уже в очереди", ephemeral: true);
                return;
            }
            else if (userDb.IsInMatch)
            {
                await FollowupAsync("Вы уже в матче", ephemeral: true);
                return;
            }

            try
            {
                queue.Add(new List<User>{ userDb });
                Log.Information($"{userGuild.DisplayName} Входит в очередь");
                await FollowupAsync($"Вы добавлены в очередь. В очереди {queue.Sum(list => list.Count)} человек", ephemeral: true);
            } catch(Exception ex)
            {
                Log.Error(ex, $"{userGuild.DisplayName} Ошибка входа в очередь");
                await FollowupAsync("Ошибка поиска", ephemeral: true);
            }

            if (!isAnalyzingRunning)
            {
                isAnalyzingRunning = true;
                _ = Task.Run(AnalyzeQueue);
            }
        }

        [SlashCommand("leave-queue", "Выйти из очереди")]
        public async Task LeaveQueue()
        {
            var userGuild = Context.User as IGuildUser;
            var userDb = await _userService.FindAsync(userGuild.Id);

            Log.Information($"{userGuild.DisplayName} Попытка выйти из очереди");

            await DeferAsync(ephemeral: true);

            var teamToRemove = queue.FirstOrDefault(team => team.Any(u => u.Id == userDb.Id));

            if (teamToRemove == null)
            {
                await FollowupAsync("Вас нет в очереди", ephemeral: true);
                return;
            }
            else if (userDb.IsInMatch)
            {
                await FollowupAsync("Вы участвуете в формировании матча, выход из очереди невозможен", ephemeral: true);
                return;
            }

            try
            {
                if (queue.Remove(teamToRemove))
                {
                    Log.Information($"{userGuild.DisplayName} Попытка выйти из очереди удалась");
                    await FollowupAsync("Вы покинули очередь", ephemeral: true);
                }
                else
                {
                    Log.Information($"{userGuild.DisplayName} Попытка выйти из очереди не удалась");
                    await FollowupAsync("Ошибка", ephemeral: true);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex,$"{userGuild.DisplayName} Попытка выйти из очереди не удалась");
                await FollowupAsync("Ошибка", ephemeral: true);
            }
        }

        private async Task AnalyzeQueue()
        {
            isAnalyzing = true;
            while (isAnalyzing)
            {
                if (queue.Sum(team => team.Count) >= MatchSize)
                {
                    Log.Information("Попытка создать матч");
                    var matchTeams = new List<List<User>>();
                    var totalPlayers = 0;

                    foreach (var team in queue.ToList())
                    {
                        if (team.Count == 2 && matchTeams.Count(t => t.Count == 2) >= 2)
                        {
                            continue;
                        }

                        if (totalPlayers + team.Count <= MatchSize)
                        {
                            matchTeams.Add(team);
                            totalPlayers += team.Count;
                            queue.Remove(team);
                        }
                        if (totalPlayers == MatchSize)
                            break;
                    }

                    if (totalPlayers == MatchSize)
                    {
                        var matchPlayers = matchTeams.SelectMany(t => t).ToList();

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var scopedUserService = scope.ServiceProvider.GetRequiredService<UserService>();
                            var scopedSolareService = scope.ServiceProvider.GetRequiredService<SolareService>();

                            foreach (var matchPlayer in matchPlayers)
                            {
                                matchPlayer.IsInMatch = true;
                                await scopedUserService.UpdateUserAsync(matchPlayer);
                            }

                            var (team1, team2) = DistributeTeams(matchTeams, TeamSize, maxEloDifference);

                            if (team1.Count != TeamSize || team2.Count != TeamSize)
                            {
                                foreach (var player in matchPlayers)
                                {
                                    player.IsInMatch = false;
                                    await scopedUserService.UpdateUserAsync(player);
                                }
                                queue.AddRange(matchTeams);
                                Log.Information("Неудачная попытка создать матч");
                                maxEloDifference += EloDifferenceIncrement;
                            }
                            else
                            {
                                var resultChannel = await _queueHelper.CreateMatchAsync(team1, team2, Context.Guild, scopedSolareService);
                                if (resultChannel != null)
                                {
                                    var team1Mentions = string.Join(", ", team1.Select(user =>
                                        $"{Context.Guild.GetUser(user.DiscordId).Mention}({user.Nickname})"));

                                    var team2Mentions = string.Join(", ", team2.Select(user =>
                                        $"{Context.Guild.GetUser(user.DiscordId).Mention}({user.Nickname})"));

                                    try
                                    {
                                        var embed = new EmbedBuilder()
                                            .WithTitle("Информация о матче")
                                            .WithDescription($"Хост {matchPlayers.FirstOrDefault().Nickname}")
                                            .AddField($"Команда 1 ", $"Капитан - {team1.FirstOrDefault().Nickname}", inline: false)
                                            .AddField("-", team1Mentions, inline: false)
                                            .AddField($"Команда 2 ", $"Капитан - {team2.FirstOrDefault().Nickname}", inline: false)
                                            .AddField("-", team2Mentions, inline: false)
                                            .WithColor(Color.Blue);

                                        var message = await resultChannel[0].SendMessageAsync(embed: embed.Build());
                                        var alarmMessage = await resultChannel[1].SendMessageAsync("Капитаны команд голосуют за исход матча!");

                                        IEmote emote1 = new Emoji("1️⃣");
                                        IEmote emote2 = new Emoji("2️⃣");
                                        await alarmMessage.AddReactionAsync(emote1);
                                        await alarmMessage.AddReactionAsync(emote2);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(ex, "Ошибка рассылки о матче");
                                    }
                                    maxEloDifference = 100;
                                }
                            }
                        }
                    }
                    else
                        Log.Information("Недостаточно игроков для создания матча");
                }
                else
                {
                    Log.Information("Недостаточно игроков в очереди, остановка анализа");
                    isAnalyzing = false;
                }
                await Task.Delay(5000);
            }
            isAnalyzingRunning = false;
        }
        private (List<User> team1, List<User> team2) DistributeTeams(List<List<User>> teams, int teamSize, int maxEloDifference)
        {
            var team1 = new List<User>();
            var team2 = new List<User>();
            var temporarilyRemovedTeams = new List<List<User>>();

            foreach (var team in teams)
            {
                if (team1.Count + team.Count <= teamSize &&
                    !team1.Any(p => team.Any(t => t.CurrentCharacter.Class.Name == p.CurrentCharacter.Class.Name)) &&
                    (team1.Count == 0 || Math.Abs(team1.Average(p => p.CurrentCharacter.Elo) - team.Average(t => t.CurrentCharacter.Elo)) <= maxEloDifference))
                {
                    team1.AddRange(team);
                    temporarilyRemovedTeams.Add(team);
                }
                else if (team2.Count + team.Count <= teamSize &&
                         !team2.Any(p => team.Any(t => t.CurrentCharacter.Class.Name == p.CurrentCharacter.Class.Name)) &&
                         (team2.Count == 0 || Math.Abs(team2.Average(p => p.CurrentCharacter.Elo) - team.Average(t => t.CurrentCharacter.Elo)) <= maxEloDifference))
                {
                    team2.AddRange(team);
                    temporarilyRemovedTeams.Add(team);
                }
            }

            if (team1.Count != teamSize || team2.Count != teamSize)
            {
                foreach (var team in temporarilyRemovedTeams)
                {
                    queue.Add(team);
                }
                team1.Clear();
                team2.Clear();
            }

            return (team1, team2);
        }
        private async Task ClearBotMessage(IDMChannel channel)
        {
            var messages = await channel.GetMessagesAsync().FlattenAsync();
            var botMessage = messages.Where(x => x.Author.Id == Context.Client.CurrentUser.Id);

            foreach (var message in botMessage)
                await message.DeleteAsync();
        }
    }
}
