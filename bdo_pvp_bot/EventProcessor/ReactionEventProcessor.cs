using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.WebSocket;
using Domain.Entities;
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

            var K = 25;

            SolareTeam winner;
            SolareTeam loser;

            switch (winnerTeam)
            {
                case 1:
                    winner = match.FirstTeam;
                    loser = match.SecondTeam;
                    break;
                case 2:
                    winner = match.SecondTeam;
                    loser = match.FirstTeam;
                    break;
                default:
                    throw new ArgumentException("Invalid winner team");
            }

            double avgEloWinner = winner.Characters.Average(c => c.Elo);
            double avgEloLoser = loser.Characters.Average(c => c.Elo);

            foreach (var winnerCharacter in winner.Characters)
            {
                var expectedScoreWinner = 1 / (1 + Math.Pow(10, (avgEloLoser - winnerCharacter.Elo) / 400.0));
                var changeInElo = K * (1 - expectedScoreWinner);
                winnerCharacter.Elo += (int)Math.Round(changeInElo);
                winnerCharacter.User.IsInMatch = false;
            }

            foreach (var loserCharacter in loser.Characters)
            {
                var expectedScoreLoser = 1 / (1 + Math.Pow(10, (avgEloWinner - loserCharacter.Elo) / 400.0));
                var changeInEloLoser = K * (0 - expectedScoreLoser);
                loserCharacter.Elo += (int)Math.Round(changeInEloLoser);
                loserCharacter.User.IsInMatch = false;
            }

            await _solareService.UpdateMatchAsync(match);
        }
    }
}
