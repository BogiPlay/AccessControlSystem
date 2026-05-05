using System;

namespace AccessControlSystem.Events
{
    public class AccessEventArgs : EventArgs
    {
        public string CardNumber { get; set; }
        public string RoomName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public AccessEventArgs(string cardNumber, string roomName, string message)
        {
            CardNumber = cardNumber;
            RoomName = roomName;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }

    // Дефинираме делегата в същия файл (или може в отделен, но тук е най-удобно)
    public delegate void AccessEventHandler(object sender, AccessEventArgs e);
}