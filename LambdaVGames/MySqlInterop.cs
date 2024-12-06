using System.Data;
using MySql.Data.MySqlClient;

namespace LambdaVGames;

public static class MySqlInterop {
    public const string DefaultDbname = "LambdaVGamesDb";
    
    public static MySqlConnection? Connection { get; private set; }
    public static string? Server { get; private set; }
    public static string? Username { get; private set; }
    public static string? Password { get; private set; }
    public static string? Database { get; private set; }
    
    public static bool IsConnectedToDatabase { get; private set; }

    public static bool IsConnectedToServer {
        get => Connection is { State: ConnectionState.Open };
    }

    public static async Task ConnectToDatabaseServer(string server, string user, string password, string database) {
        Server = server;
        Username = user;
        Password = password;
        Database = database;
        
        Connection = new MySqlConnection($"server={Server};user id={Username};password={Password};database={Database}");
        
        await Connection.OpenAsync();

        IsConnectedToDatabase = Connection.State == ConnectionState.Open;
    }

    public static async Task ConnectToDatabaseServer(string server, string user, string password) {
        Server = server;
        Username = user;
        Password = password;
        
        Connection = new MySqlConnection($"server={Server};user id={Username};password={Password};");
        
        await Connection.OpenAsync();
    }
    
    public static async Task ConnectToDatabase(string database) {
        if (!IsConnectedToServer) {
            throw new Exception("Unable to connect to database: no server connection");
        }
        
        Database = database;

        await Connection!.ChangeDatabaseAsync(database);

        IsConnectedToDatabase = Connection.State == ConnectionState.Open;
    }

    public static async Task<bool> TryConnectToDatabase(string database) {
        try {
            Database = database;

            await Connection!.ChangeDatabaseAsync(database);

            IsConnectedToDatabase = Connection.State == ConnectionState.Open;
            
            return true;
        }
        catch {
            return false;
        }
    }
    
    public static async Task CreateDatabaseAndConnect(string dbName = DefaultDbname) {
        if (!ValidateDbName(dbName)) {
            throw new Exception("Invalid db name.");
        }
        
        MySqlCommand createDbCmd = new($"CREATE DATABASE {dbName};", Connection);
        await createDbCmd.ExecuteNonQueryAsync();
        
        await Connection!.ChangeDatabaseAsync(dbName);
    }

    public static void CloseConnection() {
        Connection?.Close();

        IsConnectedToDatabase = false;
        
        Server = null;
        Username = null;
        Password = null;
        Database = null;
    }

    private static bool ValidateDbName(string dbName) {
        return !string.IsNullOrWhiteSpace(dbName) && dbName.All(char.IsLetterOrDigit);
    }
}