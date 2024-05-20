namespace Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Nickname { get; set; }
        public ulong DiscordId { get; set; }
        public bool IsInMatch { get; set; }
        public long? CurrentCharacterId { get; set; }
        public virtual Character? CurrentCharacter { get; set; }
        public virtual List<Character> Characters { get; set; } = new List<Character>();

        public override bool Equals(object obj)
        {
            if (obj is User otherUser)
            {
                return Id == otherUser.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
