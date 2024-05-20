using Microsoft.Extensions.Configuration;

namespace bdo_pvp_bot.Config
{
    public static class BotConfigLoader
    {
        public static IConfigurationRoot LoadConfig()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("botconfig.json");

            return builder.Build();
        }
    }
}
