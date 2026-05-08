using AccessControlSystem.Events;
using AccessControlSystem.Data;

namespace AccessControlSystem.Listeners
{
    public class DatabaseLogger
    {
        private readonly DatabaseManager _dbManager;

        public DatabaseLogger(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        // Слушател за успешен достъп
        public void LogSuccessToDb(object sender, AccessEventArgs e)
        {
            _dbManager.InsertLog(e.Timestamp, e.CardNumber, e.RoomName, "Granted", e.Message);
        }

        // Слушател за отказан достъп
        public void LogFailureToDb(object sender, AccessEventArgs e)
        {
            _dbManager.InsertLog(e.Timestamp, e.CardNumber, e.RoomName, "Denied", e.Message);
        }
    }
}