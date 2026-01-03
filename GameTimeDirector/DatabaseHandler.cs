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
                        UserId TEXT NOT NULL,
                        ServerIdentifier TEXT NOT NULL,
                        Minutes REAL NOT NULL,
                        PRIMARY KEY (UserId, ServerIdentifier)
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
            var serverId = Plugin.Instance.Config.ServerIdentifier;

            lock (DBLock)
            {
                using var checkCmd = _connection.CreateCommand();
                checkCmd.CommandText = "SELECT 1 FROM PlayerGameTime WHERE UserId = @userId AND ServerIdentifier = @serverId LIMIT 1;";
                checkCmd.Parameters.AddWithValue("@userId", userId);
                checkCmd.Parameters.AddWithValue("@serverId", serverId);

                var exists = checkCmd.ExecuteScalar() != null;

                if (!exists && addIfNot)
                {
                    using var insertCmd = _connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO PlayerGameTime (UserId, ServerIdentifier, Minutes) VALUES (@userId, @serverId, 0.0);";
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.Parameters.AddWithValue("@serverId", serverId);
                    insertCmd.ExecuteNonQuery();
                }

                return exists;
            }
        }

        internal static void UpdatePlayerTime(string userId, double minutesDelta, bool onlyAdd = true)
        {
            var serverId = Plugin.Instance.Config.ServerIdentifier;

            lock (DBLock)
            {
                if (onlyAdd)
                {
                    using var selectCmd = _connection.CreateCommand();
                    selectCmd.CommandText = "SELECT Minutes FROM PlayerGameTime WHERE UserId = @userId AND ServerIdentifier = @serverId;";
                    selectCmd.Parameters.AddWithValue("@userId", userId);
                    selectCmd.Parameters.AddWithValue("@serverId", serverId);

                    var result = selectCmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return;

                    var current = Convert.ToDouble(result);
                    var sum = Math.Max(0, current + minutesDelta);

                    using var updateCmd = _connection.CreateCommand();
                    updateCmd.CommandText = "UPDATE PlayerGameTime SET Minutes = @minutes WHERE UserId = @userId AND ServerIdentifier = @serverId;";
                    updateCmd.Parameters.AddWithValue("@userId", userId);
                    updateCmd.Parameters.AddWithValue("@serverId", serverId);
                    updateCmd.Parameters.AddWithValue("@minutes", sum);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    using var updateCmd = _connection.CreateCommand();
                    updateCmd.CommandText = "UPDATE PlayerGameTime SET Minutes = @minutes WHERE UserId = @userId AND ServerIdentifier = @serverId;";
                    updateCmd.Parameters.AddWithValue("@userId", userId);
                    updateCmd.Parameters.AddWithValue("@serverId", serverId);
                    updateCmd.Parameters.AddWithValue("@minutes", minutesDelta);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        internal static double? GetPlayerTime(string userId)
        {
            var serverId = Plugin.Instance.Config.ServerIdentifier;

            lock (DBLock)
            {
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "SELECT Minutes FROM PlayerGameTime WHERE UserId = @userId AND ServerIdentifier = @serverId;";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@serverId", serverId);

                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return null;

                return Convert.ToDouble(result);
            }
        }
    }
}
