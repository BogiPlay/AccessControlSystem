using System.Collections.Generic;
using System.Linq;
using AccessControlSystem.Models;
using AccessControlSystem.Events;

namespace AccessControlSystem.Controllers
{
    public class AccessController
    {
        // Дефиниране на събитията
        public event AccessEventHandler AccessGranted;
        public event AccessEventHandler AccessDenied;

        // Временни списъци за Седмица 2 (през Седмица 3 ще ползваме база данни)
        private List<AccessCard> validCards = new List<AccessCard>();
        private List<Door> doors = new List<Door>();

        public void RegisterCard(AccessCard card) => validCards.Add(card);
        public void RegisterDoor(Door door) => doors.Add(door);

        // Логика за проверка
        public void TryAccess(string cardNumber, string roomName)
        {
            var card = validCards.FirstOrDefault(c => c.CardNumber == cardNumber);
            var door = doors.FirstOrDefault(d => d.RoomName == roomName);

            if (door == null) return;

            if (card == null || !card.IsActive)
            {
                OnAccessDenied(new AccessEventArgs(cardNumber, roomName, "Невалидна или неактивна карта."));
                return;
            }

            if (card.Owner.UserRole >= door.RequiredRole)
            {
                OnAccessGranted(new AccessEventArgs(cardNumber, roomName, $"Разрешено за: {card.Owner.FirstName} ({card.Owner.UserRole})"));
            }
            else
            {
                OnAccessDenied(new AccessEventArgs(cardNumber, roomName, $"Ниско ниво на достъп. Изисква се: {door.RequiredRole}"));
            }
        }

        // Методи за вдигане на събитията
        protected virtual void OnAccessGranted(AccessEventArgs e)
        {
            AccessGranted?.Invoke(this, e);
        }

        protected virtual void OnAccessDenied(AccessEventArgs e)
        {
            AccessDenied?.Invoke(this, e);
        }
    }
}