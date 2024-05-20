namespace Domain.Entities
{
    public class SolareMatch
    {
        public long Id { get; set; }
        public long FirstTeamId { get; set; }
        public virtual SolareTeam FirstTeam { get; set; }
        public long SecondTeamId { get; set; }
        public virtual SolareTeam SecondTeam { get; set; }
        public long? WinnerId { get; set; }
        public virtual SolareTeam Winner { get; set; }
        public long? LoserId { get; set; }
        public virtual SolareTeam Loser { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
