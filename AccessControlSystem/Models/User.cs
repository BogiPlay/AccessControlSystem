namespace AccessControlSystem.Models
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role UserRole { get; set; }

        public User(string firstName, string lastName, Role role)
        {
            FirstName = firstName;
            LastName = lastName;
            UserRole = role;
        }
    }
}