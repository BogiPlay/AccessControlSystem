using System;
using AccessControlSystem.Events;

namespace AccessControlSystem.Listeners
{
    public class SecurityService
    {
        public void HandleSuspiciousActivity(object sender, AccessEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"!!! [АЛАРМЕНА СИСТЕМА] Опит за неоторизиран достъп! Врата: {e.RoomName} | Карта: {e.CardNumber}");
            Console.ResetColor();
        }
    }
}