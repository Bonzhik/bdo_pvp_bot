using bdo_pvp_bot.Commands;
using bdo_pvp_bot.Commands.Queue;
using bdo_pvp_bot.Config;
using bdo_pvp_bot.Data;
using bdo_pvp_bot.EventProcessor;
using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace bdo_pvp_bot.Helpers
{
    public static class DIHelper
    {
        public static ServiceProvider BuildBotServices()
        {
            var configuration = BotConfigLoader.LoadConfig();

            return new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>().Rest))
                .AddSingleton<CommandHandler>()
                .AddScoped<SelectMenuEventProcessor>()
                .AddScoped<UserJoinedEventProcessor>()
                .AddScoped<ReactionEventProcessor>()
                .AddDbContext<ApplicationDbContext>()
                .AddScoped<CharacterService>()
                .AddScoped<UserService>()
                .AddScoped<SolareService>()
                .AddScoped<QueueHelper>()
                .BuildServiceProvider();
        }
    }
}
