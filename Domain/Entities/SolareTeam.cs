namespace Domain.Entities
{
    public class SolareTeam
    {
        public long Id { get; set; }
        public virtual List<Character> Characters { get; set; }
    }
}
