using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using AccessControlSystem.Models;
using AccessControlSystem.Exceptions;

namespace AccessControlSystem.Data
{
    public class DatabaseManager
    {
        private readonly string connectionString = "Server=localhost;Database=AccessControlDB;User ID=root;Password=;Port=3307";

        // Проверка на връзката (демонстрира прихващане на грешки при БД)
        public void TestConnection()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("[БД] Успешна връзка с MySQL!");
                }
            }
            catch (MySqlException ex)
            {
                throw new DatabaseConnectionException($"Грешка при свързване с базата данни. XAMPP пуснат ли е? Детайли: {ex.Message}");
            }
        }

        // CRUD: Read (Взимане на всички логове за LINQ справките)
        public List<AccessLog> GetAllLogs()
        {
            List<AccessLog> logs = new List<AccessLog>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM AccessLogs";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new AccessLog
                        {
                            Id = reader.GetInt32("Id"),
                            Timestamp = reader.GetDateTime("Timestamp"),
                            CardNumber = reader.GetString("CardNumber"),
                            RoomName = reader.GetString("RoomName"),
                            EventStatus = reader.GetString("EventStatus"),
                            Message = reader.GetString("Message")
                        });
                    }
                }
            }
            return logs;
        }

        // --- LINQ ЗАДАЧИ ---

        // 1. Последни 10 събития
        public void PrintLast10Events()
        {
            var logs = GetAllLogs();
            var last10 = logs.OrderByDescending(l => l.Timestamp).Take(10);

            Console.WriteLine("\n--- ПОСЛЕДНИ 10 СЪБИТИЯ ---");
            foreach (var log in last10)
            {
                Console.WriteLine($"{log.Timestamp} | {log.RoomName} | Карта: {log.CardNumber} | Статус: {log.EventStatus}");
            }
        }

        // 2. Групиране по врата/кабинет
        public void PrintLogsGroupedByDoor()
        {
            var logs = GetAllLogs();
            var grouped = logs.GroupBy(l => l.RoomName);

            Console.WriteLine("\n--- ДОСТЪПИ, ГРУПИРАНИ ПО ВРАТА ---");
            foreach (var group in grouped)
            {
                Console.WriteLine($"Врата: {group.Key} (Общо опити: {group.Count()})");
                foreach (var log in group)
                {
                    Console.WriteLine($"  -> {log.Timestamp} | Карта: {log.CardNumber} [{log.EventStatus}]");
                }
            }
        }

        // 3. Търсене по карта
        public void SearchByCard(string cardNumber)
        {
            var logs = GetAllLogs();
            var cardLogs = logs.Where(l => l.CardNumber == cardNumber).ToList();

            Console.WriteLine($"\n--- ИСТОРИЯ ЗА КАРТА {cardNumber} ---");
            if (cardLogs.Any())
                cardLogs.ForEach(l => Console.WriteLine($"{l.Timestamp} | {l.RoomName} | {l.EventStatus}"));
            else
                Console.WriteLine("Няма намерени записи.");
        }
    }
}