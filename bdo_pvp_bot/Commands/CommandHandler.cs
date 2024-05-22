using bdo_pvp_bot.EventProcessor;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;

namespace bdo_pvp_bot.Commands
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly IServiceScopeFactory _scopeFactory;
        public CommandHandler(IServiceProvider services, IServiceScopeFactory scopeFactory)
        {
            _services = services;
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<InteractionService>();
            _scopeFactory = scopeFactory;
        }

        public async Task InitAsync()
        {
            _commands.Log += LogAsync;

            _client.Ready += InitCommands;
            _client.Ready += LobbyMessage;

            _client.UserJoined += HandleUserJoin;

            _client.InteractionCreated += HandleInteraction;
            _client.MessageReceived += HandleMessageReceived;

            _client.SelectMenuExecuted += HandleSelectMenuAsync;

            _client.ReactionAdded += HandleReactionAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task InitCommands() => await _commands.RegisterCommandsToGuildAsync(1239716923428835418, true);

        private async Task HandleUserJoin(SocketGuildUser user)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<UserJoinedEventProcessor>();
                await processor.CheckUser(user);
            }
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            const ulong lobbyId = 1240660490175254538;

            if (message.Author.IsBot)
                return;

            switch (message.Channel.Id)
            {
                case lobbyId:
                    await message.DeleteAsync();
                    break;
                default:
                    break;
            }
        }
        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> messageCache, Cacheable<IMessageChannel, ulong> channelCache, SocketReaction reaction)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<ReactionEventProcessor>();
                var reactionChannel = await channelCache.GetOrDownloadAsync() as SocketTextChannel;
                if (reactionChannel == null)
                    return;
                var category = reactionChannel.Category;
                Regex redex = new Regex(@"^MatchId-\d+$");
                if (redex.IsMatch(category.Name))
                {
                    var message = await messageCache.GetOrDownloadAsync();
                    await processor.FinishMatch(message, reactionChannel, reaction);
                }
                return;
            }
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _commands.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }
        private async Task HandleSelectMenuAsync(SocketMessageComponent component)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                Log.Information($"Пришел ивент {component.Data.CustomId}");
                var processor = scope.ServiceProvider.GetRequiredService<SelectMenuEventProcessor>();
                Regex createCharRegex = new Regex(@"^createCharId-\d+-\d+$");
                Regex ccreateCharRedex = new Regex(@"^ccreateCharId-\d+-\d+$");

                if (createCharRegex.IsMatch(component.Data.CustomId))
                    await processor.ClassMenuProcess(component);
                else if (component.Data.CustomId == "deleteChar")
                    await processor.DeleteCharProcess(component);
                else if (component.Data.CustomId == "selectChar")
                    await processor.SelectClassMenuProcess(component);

            }
        }

        private async Task LobbyMessage()
        {
            ulong channelId = 1242487556625403926;
            var channel = _client.GetChannel(channelId) as IMessageChannel;
            var messageCheck = await channel.GetMessagesAsync().ToListAsync();

            var messages = messageCheck.SelectMany(batch => batch).ToList();

            var embed = new EmbedBuilder()
                .WithTitle("Личный кабинет")
                .AddField("Для регистрации введите фамилию персонажа", "(/registration)", false)
                .AddField("Добавьте персонажа", "(/add-character)", false)
                .AddField("Изменить фамилию", "(/update-character)", false)
                .AddField("Выберите текущего персонажа", "(/select-character)", false)
                .AddField("Если ввели че-то не так", "(/delete-character)", false)
                .AddField("Статистика о текущем персонаже", "(/stat)", false)
                .AddField("Соло поиск", "(/queue-solo)", false)
                .AddField("Выйти из поиска", "(/queue-leave)", false)
                .WithColor(Color.Blue);

            if (messages.Count == 0)
            {
                await channel.SendMessageAsync(embed: embed.Build());
            }
        }
        private static async Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };
            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }
    }
}
