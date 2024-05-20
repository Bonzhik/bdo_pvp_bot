using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace bdo_pvp_bot.EventProcessor
{
    public class ReactionEventProcessor
    {
        private readonly SolareService _solareService;
        public ReactionEventProcessor(SolareService solareService)
        {
            _solareService = solareService;
        }
        public async Task FinishMatch(IUserMessage message, IMessageChannel channel, SocketReaction reaction)
        {
            if (message == null || channel == null || reaction == null)
            {
                return;
            }

            var team1VictoryEmoji = new Emoji("1️⃣");
            var team2VictoryEmoji = new Emoji("2️⃣");

            var guildChannel = channel as IGuildChannel;
            if (guildChannel == null)
                return;

            var reactions = message.Reactions;

            if (reactions.ContainsKey(team1VictoryEmoji) && reactions[team1VictoryEmoji].ReactionCount >= 3)
            {
                await CalculateElo(channel, 1);
                await ClearAfterMatch(channel, message);
            }
            else if (reactions.ContainsKey(team2VictoryEmoji) && reactions[team2VictoryEmoji].ReactionCount >= 3)
            {
                await CalculateElo(channel, 2);
                await ClearAfterMatch(channel, message);
            }
        }
        private async Task ClearAfterMatch(IMessageChannel channel, IUserMessage message)
        {
            var textChannel = channel as ITextChannel;
            var category = await textChannel.GetCategoryAsync() as SocketCategoryChannel;

            foreach (var ch in category.Channels)
            {
                await ch.DeleteAsync();
            }
            await category.DeleteAsync();
        }
        private async Task CalculateElo(IMessageChannel channel, int winnerTeam)
        {
            var textChannel = channel as ITextChannel;
            var category = await textChannel.GetCategoryAsync() as SocketCategoryChannel;

            var matchId = long.Parse(category.Name.Substring(8));
            var match = await _solareService.FindAsync(matchId);

            Log.Information($"Рассчет очков для матча {matchId}");

            switch (winnerTeam)
            {
                case 1:
                    match.Winner = match.FirstTeam;
                    match.Loser = match.SecondTeam;
                    foreach (var winner in match.Winner.Characters)
                    {
                        winner.Elo += 20;
                        winner.User.IsInMatch = false;

                    }
                    foreach (var loser in match.Loser.Characters)
                    {
                        loser.Elo -= 20;
                        loser.User.IsInMatch = false;
                    }
                    await _solareService.UpdateMatchAsync(match);
                    break;
                case 2:
                    match.Winner = match.SecondTeam;
                    match.Loser = match.FirstTeam;
                    foreach (var winner in match.Winner.Characters)
                    {
                        winner.Elo += 20;
                        winner.User.IsInMatch = false;
                    }
                    foreach (var loser in match.Loser.Characters)
                    {
                        loser.Elo -= 20;
                        loser.User.IsInMatch = false;
                    }
                    await _solareService.UpdateMatchAsync(match);
                    break;
            }
        }
    }
}
