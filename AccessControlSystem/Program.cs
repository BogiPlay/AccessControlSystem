using System;
using AccessControlSystem.Models;
using AccessControlSystem.Events;
using AccessControlSystem.Controllers;
using AccessControlSystem.Listeners;

namespace AccessControlSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            // Оправяме кирилицата в конзолата
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 1. Създаваме основните обекти
            AccessController controller = new AccessController();
            FileLogger fileLogger = new FileLogger();
            SecurityService securitySystem = new SecurityService();

            // 2. АБОНИРАНЕ НА СЛУШАТЕЛИТЕ ЗА СЪБИТИЯТА (Изключително важно за изискванията)
            controller.AccessGranted += fileLogger.LogSuccess;

            // Тук виждаме 2 слушателя към 1 събитие!
            controller.AccessDenied += fileLogger.LogFailure;
            controller.AccessDenied += securitySystem.HandleSuspiciousActivity;

            // 3. Създаване на малко фиктивни данни
            User student = new User("Иван", "Иванов", Role.Student);
            User admin = new User("Георги", "Димитров", Role.Admin);

            AccessCard cardStudent = new AccessCard("CARD-111", student);
            AccessCard cardAdmin = new AccessCard("CARD-999", admin);

            Door lab = new Door("Компютърна зала", Role.Student);
            Door serverRoom = new Door("Сървърно помещение", Role.Admin);

            controller.RegisterCard(cardStudent);
            controller.RegisterCard(cardAdmin);
            controller.RegisterDoor(lab);
            controller.RegisterDoor(serverRoom);

            // 4. Демонстрация
            Console.WriteLine("--- СИСТЕМА ЗА КОНТРОЛ НА ДОСТЪПА ---\n");

            Console.WriteLine("Опит 1: Ученик опитва да влезе в Компютърната зала...");
            controller.TryAccess("CARD-111", "Компютърна зала");

            Console.WriteLine("\nОпит 2: Ученик опитва да влезе в Сървърното помещение...");
            controller.TryAccess("CARD-111", "Сървърно помещение");

            Console.WriteLine("\nОпит 3: Админ опитва да влезе в Сървърното помещение...");
            controller.TryAccess("CARD-999", "Сървърно помещение");

            Console.ReadLine();
        }
    }
}