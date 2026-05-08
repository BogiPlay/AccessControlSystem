using System;
using AccessControlSystem.Data;
using AccessControlSystem.Exceptions;

namespace AccessControlSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            DatabaseManager dbManager = new DatabaseManager();
            FileManager fileManager = new FileManager();

            Console.WriteLine("--- СТАРТИРАНЕ НА СИСТЕМАТА ЗА КОНТРОЛ НА ДОСТЪПА ---");

            // 1. Демонстрация на обработка на изключения (try-catch)
            try
            {
                dbManager.TestConnection();
            }
            catch (DatabaseConnectionException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("Натисни Enter за изход...");
                Console.ReadLine();
                return; // Спираме програмата, ако няма връзка с базата
            }

            // 2. Демонстрация на LINQ справки
            dbManager.PrintLast10Events();
            dbManager.PrintLogsGroupedByDoor();
            dbManager.SearchByCard("CARD-111");

            // 3. Демонстрация на експорт във файл
            var allLogs = dbManager.GetAllLogs();
            fileManager.ExportLogsToCsv(allLogs);

            Console.WriteLine("\nДемонстрацията завърши. Натиснете Enter.");
            Console.ReadLine();
        }
    }
}