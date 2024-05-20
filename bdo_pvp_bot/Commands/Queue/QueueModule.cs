using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.Interactions;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace bdo_pvp_bot.Commands.Queue
{
    public class QueueModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SolareService _solareService;
        private readonly UserService _userService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly QueueHelper _queueHelper;

        private static readonly List<User> queue = new List<User>();
        private const int TeamSize = 3;
        private const int MatchSize = TeamSize * 2;
        private static bool isAnalyzing = false;

        public QueueModule(SolareService solareService, UserService userService, IServiceScopeFactory scopeFactory, QueueHelper queueHelper)
        {
            _solareService = solareService;
            _userService = userService;
            _scopeFactory = scopeFactory;
            _queueHelper = queueHelper;

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
            else if (queue.Any(u => u.DiscordId == userGuild.Id))
            {
                await FollowupAsync("Вы уже в очереди -_-", ephemeral: true);
                return;
            }
            else if (userDb.IsInMatch)
            {
                await FollowupAsync("Вы уже в матче -_-", ephemeral: true);
                return;
            }

            try
            {
                queue.Add(userDb);
                Log.Information($"{userGuild.DisplayName} Входит в очередь");
                await FollowupAsync("Вы добавлены в очередь", ephemeral: true);
            } catch(Exception ex)
            {
                Log.Error(ex, $"{userGuild.DisplayName} Ошибка входа в очередь");
                await FollowupAsync("Ошибка поиска", ephemeral: true);
            }

            if (!isAnalyzing)
            {
                isAnalyzing = true;
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

            if (!queue.Any(u => u.Id == userDb.Id))
            {
                await FollowupAsync("Вас нет в очереди -_-", ephemeral: true);
                return;
            }
            else if (userDb.IsInMatch)
            {
                await FollowupAsync("Вы участвуете в формировании матча, выход из очереди невозможен", ephemeral: true);
                return;
            }
            try
            {
                if (queue.Remove(userDb))
                {
                    Log.Information($"{userGuild.DisplayName} Попытка выйти из очереди");
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
            while (true)
            {
                if (queue.Count >= MatchSize)
                {
                    Log.Information("Попытка создать матч");
                    var matchPlayers = queue.Take(MatchSize).ToList();
                    queue.RemoveRange(0, MatchSize);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var scopedUserService = scope.ServiceProvider.GetRequiredService<UserService>();
                        var scopedSolareService = scope.ServiceProvider.GetRequiredService<SolareService>();

                        foreach (var matchPlayer in matchPlayers)
                        {
                            matchPlayer.IsInMatch = true;
                            await scopedUserService.UpdateUserAsync(matchPlayer);
                        }

                        var (team1, team2) = DistributePlayers(matchPlayers, TeamSize, 100);

                        if (team1.Count != TeamSize || team2.Count != TeamSize)
                        {
                            foreach (var player in matchPlayers)
                            {
                                player.IsInMatch = false;
                                await scopedUserService.UpdateUserAsync(player);
                            }
                            queue.AddRange(matchPlayers);
                            Log.Information("Неудачная попытка создать матч");
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

                            }
                        }
                    }
                }
                await Task.Delay(5000);
            }
        }
        private (List<User> team1, List<User> team2) DistributePlayers(List<User> players, int teamSize, int maxEloDifference)
        {
            var team1 = new List<User>();
            var team2 = new List<User>();

            foreach (var player in players)
            {
                var test1 = team1.Count < teamSize;
                var test2 = !team1.Any(p => p.CurrentCharacter.Class.Name == player.CurrentCharacter.Class.Name && p.CurrentCharacter.ClassType == player.CurrentCharacter.ClassType);
                var test3 = (team1.Count == 0 || Math.Abs(team1.Average(p => p.CurrentCharacter.Elo) - player.CurrentCharacter.Elo) <= maxEloDifference);
                if (test1 && test2 &&
                    test3)
                {
                    team1.Add(player);
                }
                else if (team2.Count < teamSize && !team2.Any(p => p.CurrentCharacter.Class.Name == player.CurrentCharacter.Class.Name && p.CurrentCharacter.ClassType == player.CurrentCharacter.ClassType) &&
                         (team2.Count == 0 || Math.Abs(team2.Average(p => p.CurrentCharacter.Elo) - player.CurrentCharacter.Elo) <= maxEloDifference))
                {
                    team2.Add(player);
                }
            }

            return (team1, team2);
        }
    }
}
