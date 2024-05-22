

namespace Domain.Entities
{
    public class Character
    {
        public long Id { get; set; }
        public int Elo { get; set; }
        public virtual CharacterClass? Class { get; set; }
        public virtual User User { get; set; }
        public virtual List<SolareTeam> Teams { get; set; } = new List<SolareTeam>();
        public virtual List<OneVsOneMatch> OneVsOneMatches { get; set; } = new List<OneVsOneMatch>();
    }
}
