using AccessControlSystem.Exceptions;
using AccessControlSystem.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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


        // Метод за добавяне на нов потребител и карта
        public void AddUserWithCard(string firstName, string lastName, int role, string cardNumber)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // 1. Добавяне на потребител
                string userQuery = "INSERT INTO Users (FirstName, LastName, UserRole) VALUES (@fn, @ln, @role); SELECT LAST_INSERT_ID();";
                MySqlCommand userCmd = new MySqlCommand(userQuery, conn);
                userCmd.Parameters.AddWithValue("@fn", firstName);
                userCmd.Parameters.AddWithValue("@ln", lastName);
                userCmd.Parameters.AddWithValue("@role", role);

                int newUserId = Convert.ToInt32(userCmd.ExecuteScalar());

                // 2. Добавяне на карта за този потребител
                string cardQuery = "INSERT INTO Cards (CardNumber, UserId, IsActive) VALUES (@card, @uid, 1)";
                MySqlCommand cardCmd = new MySqlCommand(cardQuery, conn);
                cardCmd.Parameters.AddWithValue("@card", cardNumber);
                cardCmd.Parameters.AddWithValue("@uid", newUserId);
                cardCmd.ExecuteNonQuery();

                Console.WriteLine("\n[БД] Потребителят и картата са добавени успешно!");
            }
        }

        // Метод за добавяне на нова врата
        public void AddDoor(string roomName, int requiredRole)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Doors (RoomName, RequiredRole) VALUES (@room, @role)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@room", roomName);
                cmd.Parameters.AddWithValue("@role", requiredRole);
                cmd.ExecuteNonQuery();
                Console.WriteLine("\n[БД] Вратата е регистрирана успешно!");
            }
        }

        // Метод за запис на ново събитие (лог) в базата данни
        public void InsertLog(DateTime timestamp, string cardNumber, string roomName, string eventStatus, string message)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO AccessLogs (Timestamp, CardNumber, RoomName, EventStatus, Message) VALUES (@ts, @card, @room, @status, @msg)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ts", timestamp);
                    cmd.Parameters.AddWithValue("@card", cardNumber);
                    cmd.Parameters.AddWithValue("@room", roomName);
                    cmd.Parameters.AddWithValue("@status", eventStatus);
                    cmd.Parameters.AddWithValue("@msg", message);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- ПРОВЕРКИ (VALIDATION) ---

        public bool DoesCardExist(string cardNumber)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Cards WHERE CardNumber = @card", conn);
            cmd.Parameters.AddWithValue("@card", cardNumber);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool DoesDoorExist(string roomName)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Doors WHERE RoomName = @room", conn);
            cmd.Parameters.AddWithValue("@room", roomName);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        // --- СПИСЪЦИ (READ) ---

        public DataTable GetAllUsersWithCards()
        {
            DataTable dt = new DataTable();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string query = @"SELECT u.FirstName, u.LastName, u.UserRole, c.CardNumber, c.IsActive 
                             FROM Users u JOIN Cards c ON u.Id = c.UserId";
            using var adapter = new MySqlDataAdapter(query, conn);
            adapter.Fill(dt);
            return dt;
        }

        public DataTable GetAllDoors()
        {
            DataTable dt = new DataTable();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var adapter = new MySqlDataAdapter("SELECT RoomName, RequiredRole FROM Doors", conn);
            adapter.Fill(dt);
            return dt;
        }

        // --- СИНХРОНИЗАЦИЯ ЗА ACCESS CONTROLLER ---

        public AccessCard GetCardFromDb(string cardNumber)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string query = @"SELECT u.FirstName, u.LastName, u.UserRole, c.CardNumber 
                             FROM Cards c JOIN Users u ON c.UserId = u.Id 
                             WHERE c.CardNumber = @card AND c.IsActive = 1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@card", cardNumber);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var user = new User(reader.GetString(0), reader.GetString(1), (Role)reader.GetInt32(2));
                return new AccessCard(reader.GetString(3), user);
            }
            return null;
        }

        public Door GetDoorFromDb(string roomName)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT RoomName, RequiredRole FROM Doors WHERE RoomName = @room", conn);
            cmd.Parameters.AddWithValue("@room", roomName);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Door(reader.GetString(0), (Role)reader.GetInt32(1));
            }
            return null;
        }

        // Метод за изтриване на потребител по номер на неговата карта
        public void DeleteUserByCard(string cardNumber)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            // Тъй като имаме ON DELETE CASCADE в БД, изтриването на потребителя 
            // автоматично ще премахне и неговата карта в таблица Cards.
            string query = "DELETE FROM Users WHERE Id = (SELECT UserId FROM Cards WHERE CardNumber = @card)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@card", cardNumber);
            cmd.ExecuteNonQuery();
        }

        // Метод за изтриване на врата
        public void DeleteDoor(string roomName)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM Doors WHERE RoomName = @room", conn);
            cmd.Parameters.AddWithValue("@room", roomName);
            cmd.ExecuteNonQuery();
        }


    }
}