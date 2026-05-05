using System;
using AccessControlSystem.Events;

namespace AccessControlSystem.Listeners
{
    public class FileLogger
    {
        public void LogSuccess(object sender, AccessEventArgs e)
        {
            // В Седмица 3 тук ще добавим запис в .txt файл
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[ФАЙЛОВ ЛОГ - УСПЕХ] {e.Timestamp}: Карта {e.CardNumber} влезе в {e.RoomName}. {e.Message}");
            Console.ResetColor();
        }

        public void LogFailure(object sender, AccessEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[ФАЙЛОВ ЛОГ - ОТКАЗ] {e.Timestamp}: Карта {e.CardNumber} получи отказ за {e.RoomName}. {e.Message}");
            Console.ResetColor();
        }
    }
}