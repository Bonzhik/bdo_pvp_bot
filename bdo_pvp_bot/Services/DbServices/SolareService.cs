using bdo_pvp_bot.Data;
using Domain.Entities;
using Serilog;

namespace bdo_pvp_bot.Services.DbServices
{
    public class SolareService
    {
        private readonly ApplicationDbContext _context;
        public SolareService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<SolareMatch> CreateMatchAsync(List<User> firstTeam, List<User> secondTeam)
        {
            var team1 = new SolareTeam { Characters = firstTeam.Select(c => c.CurrentCharacter).ToList() };
            var team2 = new SolareTeam { Characters = secondTeam.Select(c => c.CurrentCharacter).ToList() };
            var match = new SolareMatch()
            {
                FirstTeam = team1,
                SecondTeam = team2,
                StartAt = DateTime.UtcNow
            };
            try
            {
                await Console.Out.WriteLineAsync();
                _context.SolareMatches.Add(match);
                _context.SaveChanges();
                return match;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при создании матча");
                return null;
            }
        }
        public async Task<SolareMatch> UpdateMatchAsync(SolareMatch solareMatch)
        {
            try
            {
                _context.SolareMatches.Update(solareMatch);
                await _context.SaveChangesAsync();
                return solareMatch;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка при сохранении результатов матча {solareMatch.Id}");
                return null;
            }
        }
        public async Task<SolareMatch> FindAsync(long id) => await _context.SolareMatches.FindAsync(id);
    }
}
