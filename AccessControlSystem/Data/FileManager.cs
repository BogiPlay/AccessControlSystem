using System;
using System.Collections.Generic;
using System.IO;
using AccessControlSystem.Models;

namespace AccessControlSystem.Data
{
    public class FileManager
    {
        // Запис на логовете в .csv файл
        public void ExportLogsToCsv(List<AccessLog> logs, string filePath = "LogsExport.csv")
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Id,Timestamp,CardNumber,RoomName,EventStatus,Message");
                    foreach (var log in logs)
                    {
                        writer.WriteLine($"{log.Id},{log.Timestamp},{log.CardNumber},{log.RoomName},{log.EventStatus},{log.Message}");
                    }
                }
                Console.WriteLine($"\n[ФАЙЛ] Успешно експортирани {logs.Count} записа в {filePath}");
            }
            catch (Exception ex) // Обработка на грешки при файл
            {
                Console.WriteLine($"\n[ГРЕШКА] Проблем при запис на файла: {ex.Message}");
            }
        }
    }
}