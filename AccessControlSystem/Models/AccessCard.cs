namespace AccessControlSystem.Models
{
    public class AccessCard
    {
        public string CardNumber { get; set; }
        public User Owner { get; set; }
        public bool IsActive { get; set; }

        public AccessCard(string cardNumber, User owner)
        {
            CardNumber = cardNumber;
            Owner = owner;
            IsActive = true;
        }
    }
}