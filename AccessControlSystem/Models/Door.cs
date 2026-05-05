namespace AccessControlSystem.Models
{
    public class Door
    {
        public string RoomName { get; set; }
        public Role RequiredRole { get; set; }

        public Door(string roomName, Role requiredRole)
        {
            RoomName = roomName;
            RequiredRole = requiredRole;
        }
    }
}