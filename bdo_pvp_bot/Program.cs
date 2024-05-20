using bdo_pvp_bot;
using bdo_pvp_bot.Helpers;
using Serilog;
using Serilog.Events;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = DIHelper.BuildBotServices();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Уровень логирования для всех категорий Microsoft
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information) // Уровень логирования для EF Core
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information) // Уровень логирования для команд БД
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var bot = new DiscordBot(services);
        await bot.InitAsync();
    }
}