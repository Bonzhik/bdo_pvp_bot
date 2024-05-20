namespace Domain.Entities
{
    public class CharacterClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public virtual List<Character> Classes { get; set; }
    }
}
