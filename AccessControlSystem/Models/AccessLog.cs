using System;

namespace AccessControlSystem.Models
{
    public class AccessLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string CardNumber { get; set; }
        public string RoomName { get; set; }
        public string EventStatus { get; set; }
        public string Message { get; set; }
    }
}