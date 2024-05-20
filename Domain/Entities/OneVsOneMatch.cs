namespace Domain.Entities
{
    public class OneVsOneMatch
    {
        public long Id { get; set; }
        public virtual User FirstPlayer { get; set; }
        public virtual User SecondPlayer { get; set; }
        public virtual User? Winner { get; set; }
        public virtual User? Loser { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
