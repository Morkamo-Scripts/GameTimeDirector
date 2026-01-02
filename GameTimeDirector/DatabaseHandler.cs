using System;
using System.IO;
using Exiled.API.Features;
using System.Data.SQLite;

namespace GameTimeDirector
{
    public static class DatabaseHandler
    {
        private static readonly object DBLock = new();
        private static SQLiteConnection _connection;

        internal static void InitializeDatabase()
        {
            lock (DBLock)
            {
                if (_connection != null)
                    return;

                var pluginFolder = Path.Combine(Paths.Plugins, Plugin.Instance.Name);
                Directory.CreateDirectory(pluginFolder);

                var dbPath = Path.Combine(pluginFolder, "GameTimeDatabase.db");
                var connectionString = $"Data Source={dbPath};Version=3;";

                _connection = new SQLiteConnection(connectionString);
                _connection.Open();

                using var command = _connection.CreateCommand();
                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS PlayerGameTime (
                        UserId TEXT NOT NULL PRIMARY KEY,
                        Minutes REAL NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }

        internal static void Shutdown()
        {
            lock (DBLock)
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
        }

        internal static bool CheckPlayerInDatabase(string userId, bool addIfNot = false)
        {
            lock (DBLock)
            {
                using var checkCmd = _connection.CreateCommand();
                checkCmd.CommandText = "SELECT 1 FROM PlayerGameTime WHERE UserId = @userId LIMIT 1;";
                checkCmd.Parameters.AddWithValue("@userId", userId);

                var exists = checkCmd.ExecuteScalar() != null;

                if (!exists && addIfNot)
                {
                    using var insertCmd = _connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO PlayerGameTime (UserId, Minutes) VALUES (@userId, 0.0);";
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.ExecuteNonQuery();
                }

                return exists;
            }
        }

        internal static void UpdatePlayerTime(string userId, double minutesDelta, bool onlyAdd = true)
        {
            lock (DBLock)
            {
                if (onlyAdd)
                {
                    using var selectCmd = _connection.CreateCommand();
                    selectCmd.CommandText = "SELECT Minutes FROM PlayerGameTime WHERE UserId = @userId;";
                    selectCmd.Parameters.AddWithValue("@userId", userId);

                    var result = selectCmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return;

                    var current = Convert.ToDouble(result);
                    var sum = Math.Max(0, current + minutesDelta);

                    using var updateCmd = _connection.CreateCommand();
                    updateCmd.CommandText = "UPDATE PlayerGameTime SET Minutes = @minutes WHERE UserId = @userId;";
                    updateCmd.Parameters.AddWithValue("@userId", userId);
                    updateCmd.Parameters.AddWithValue("@minutes", sum);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    using var updateCmd = _connection.CreateCommand();
                    updateCmd.CommandText = "UPDATE PlayerGameTime SET Minutes = @minutes WHERE UserId = @userId;";
                    updateCmd.Parameters.AddWithValue("@userId", userId);
                    updateCmd.Parameters.AddWithValue("@minutes", minutesDelta);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        internal static double? GetPlayerTime(string userId)
        {
            lock (DBLock)
            {
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "SELECT Minutes FROM PlayerGameTime WHERE UserId = @userId;";
                cmd.Parameters.AddWithValue("@userId", userId);

                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return null;

                return Convert.ToDouble(result);
            }
        }
    }
}
