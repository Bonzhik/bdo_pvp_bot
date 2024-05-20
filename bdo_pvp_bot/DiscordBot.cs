    using bdo_pvp_bot.Commands;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace bdo_pvp_bot
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _handler;
        private readonly IConfiguration _configuration;
        public DiscordBot(IServiceProvider provider)
        {
            _client = provider.GetRequiredService<DiscordSocketClient>();
            _configuration = provider.GetRequiredService<IConfiguration>();
            _handler = provider.GetRequiredService<CommandHandler>();
        }
        public async Task InitAsync()
        {
            _client.Log += LogAsync;

            await _client.LoginAsync(TokenType.Bot, _configuration["DISCORD_TOKEN"]);
            await _client.StartAsync();
            await _handler.InitAsync();

            await Task.Delay(-1);
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
