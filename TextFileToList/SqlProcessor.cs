using System;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace TextFileToList
{
    public static class SqlProcessor
    {
        public static async Task BackupDBAsync(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            try
            {
                // Create a SQL connection
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    ServerConnection serverConnection = new ServerConnection(sqlConnection);
                    Server server = new Server(serverConnection);

                    // Get the database name from the connection string
                    string databaseName = sqlConnection.Database;

                    if (string.IsNullOrWhiteSpace(databaseName))
                    {
                        throw new InvalidOperationException("Database name cannot be determined from the connection string.");
                    }

                    Database database = server.Databases[databaseName];

                    if (database == null)
                    {
                        throw new InvalidOperationException($"Database '{databaseName}' does not exist on the server.");
                    }

                    // Define the backup
                    Backup backup = new Backup
                    {
                        Action = BackupActionType.Database,
                        Database = databaseName
                    };

                    // Set the backup file path
                    string backupFilePath = $"{databaseName}_{DateTime.Now:yyyyMMddHHmmss}.bak";
                    backup.Devices.AddDevice(backupFilePath, DeviceType.File);
                    backup.Initialize = true;

                    // Perform the backup
                    Console.WriteLine("Starting database backup...");
                    backup.SqlBackupAsync(server);
                    Console.WriteLine($"Backup completed successfully. File: {backupFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during the backup process: {ex.Message}");
                throw;
            }
        }

        public static async Task BackupDBWithProgressAsync(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    ServerConnection serverConnection = new ServerConnection(sqlConnection);
                    Server server = new Server(serverConnection);

                    string databaseName = sqlConnection.Database;

                    if (string.IsNullOrWhiteSpace(databaseName))
                    {
                        throw new InvalidOperationException("Database name cannot be determined from the connection string.");
                    }

                    Database database = server.Databases[databaseName];

                    if (database == null)
                    {
                        throw new InvalidOperationException($"Database '{databaseName}' does not exist on the server.");
                    }

                    Backup backup = new Backup
                    {
                        Action = BackupActionType.Database,
                        Database = databaseName
                    };

                    string backupFilePath = $"{databaseName}_{DateTime.Now:yyyyMMddHHmmss}.bak";
                    backup.Devices.AddDevice(backupFilePath, DeviceType.File);
                    backup.Initialize = true;

                    Console.WriteLine("Starting database backup...");

                    // Subscribe to the PercentComplete event
                    backup.PercentComplete += (sender, e) =>
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Backup progress: {e.Percent}%");
                    };

                    await Task.Run(() => backup.SqlBackup(server));

                    Console.WriteLine("\nBackup completed successfully.");
                    Console.WriteLine($"Backup file: {backupFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during the backup process: {ex.Message}");
                throw;
            }
        }
    }
}
