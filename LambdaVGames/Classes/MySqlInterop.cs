using System.Data;
using MySql.Data.MySqlClient;

namespace LambdaVGames.Classes;

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
        
        IsConnectedToDatabase = Connection.State == ConnectionState.Open;

        await CreateDbSchema();
    }
    
    public static async Task<bool> ValidateDbSchema() {
        if (!IsConnectedToServer) {
            throw new Exception("Unable to retrieve schema: no server connection.");
        }
        if (!IsConnectedToDatabase) {
            throw new Exception("Unable retrieve schema: No database connection.");
        }
        
        DataTable tables = await Connection!.GetSchemaAsync("Tables");
        DataTable columns = await Connection.GetSchemaAsync("Columns");

        DataSet reqSchema = GetRequiredSchema();
        
        foreach (DataTable reqTable in reqSchema.Tables) {
            // Validate table existence.
            DataRow[] tableRows = tables.Select($"TABLE_NAME = '{reqTable.TableName}'");
            
            // Table is missing.
            if (tableRows.Length == 0) {
                return false;
            }
            
            // Validate columns.
            foreach (DataColumn reqColumn in reqTable.Columns) {
                DataRow[] columnRows = columns.Select(
                    $"TABLE_NAME = '{reqTable.TableName}' AND COLUMN_NAME = '{reqColumn.ColumnName}'");

                // Column is missing
                if (columnRows.Length == 0) {
                    return false;
                }
                
                // Validate column.
                string? dataType = columnRows[0]["DATA_TYPE"].ToString();
                
                // Invalid datatype.
                if (dataType != GetSqlDatatype(reqColumn.DataType)) {
                    return false;
                }
                
                // Validate NOT NULL.
                string isNullable = columnRows[0]["IS_NULLABLE"].ToString()!;
                bool isNotNull = isNullable == "NO";
                
                // Invalid NOT NULL attribute.
                if (isNotNull != !reqColumn.AllowDBNull) {
                    return false;
                }
                
                // Validate EXTRA for AUTO_INCREMENT
                string extra = columnRows[0]["EXTRA"].ToString()!;
                bool isAutoIncrement = extra == "auto_increment";
                
                // Invalid AUTO_INCREMENT attribute.
                if (isAutoIncrement != reqColumn.AutoIncrement) {
                    return false;
                }
            }

            // Check for additional tables.
            DataRow[] extraColumns = columns.Select($"TABLE_NAME = '{reqTable.TableName}'")
                .Where(row => !reqTable.Columns.Contains(row["COLUMN_NAME"].ToString()!))
                .ToArray();

            // Invalid additional tables.
            if (extraColumns.Length > 0) {
                return false;
            }
            
            DataTable pkTable = await Connection.GetSchemaAsync("IndexColumns", [null, null, reqTable.TableName, "PRIMARY"]);

            // Invalid primary key(s).
            if (pkTable.Rows.Count != reqTable.PrimaryKey.Length) {
                return false;
            }
            
            if (pkTable.Rows.Count > 0) {
                // Retrieve sorted pk lists.
                List<string?> pkColumns = pkTable.AsEnumerable()
                    .Select(row => row["COLUMN_NAME"].ToString())
                    .OrderBy(col => col)
                    .ToList();

                List<string> reqPkColumns = reqTable.PrimaryKey
                    .Select(col => col.ColumnName)
                    .OrderBy(col => col)
                    .ToList();

                // Invalid primary key(s).
                if (!pkColumns.SequenceEqual(reqPkColumns)) {
                    return false;
                }
            }
        }

        // Check for additional tables.
        foreach (DataRow row in tables.Rows) {
            string tableName = row["TABLE_NAME"].ToString()!;
            
            // Invalid additional table.
            if (!reqSchema.Tables.Contains(tableName)) {
                return false;
            }
        }

        // Schema is valid.
        return true;
    }

    public static async Task CreateDbSchema() {
        if (!IsConnectedToServer) {
            throw new Exception("Unable to create schema: no server connection.");
        }
        if (!IsConnectedToDatabase) {
            throw new Exception("Unable create schema: No database connection.");
        }
        
        DataTable tables = await Connection!.GetSchemaAsync("Tables");

        // Drop all tables -> temporarily disable FK checks.
        await using (MySqlCommand disableFkChecks = new("SET foreign_key_checks = 1;", Connection)) {
            await disableFkChecks.ExecuteNonQueryAsync();
            
            foreach (DataRow row in tables.Rows) {
                string tableName = row["TABLE_NAME"].ToString()!;
            
                MySqlCommand dropTableCmd = new($"DROP TABLE {tableName};", Connection);
                await dropTableCmd.ExecuteNonQueryAsync();
            }
        }
        
        MySqlCommand createNewTableCmd = new("""
                                             CREATE TABLE Games (
                                                 Id INT PRIMARY KEY AUTO_INCREMENT,
                                                 Name TEXT NOT NULL,
                                                 Category TEXT NOT NULL,
                                                 Description TEXT NOT NULL,
                                                 Price FLOAT NOT NULL,
                                                 ReleaseDate DATETIME NOT NULL,
                                                 Multiplayer BOOLEAN NOT NULL
                                             );
                                            """, Connection);
        await createNewTableCmd.ExecuteNonQueryAsync();
    }

    public static async Task QueryDatabase(ICollection<Game> collection, string filter = "") {
        collection.Clear();

        MySqlCommand pullAll = new($"SELECT * FROM Games {filter};", Connection);

        await using MySqlDataReader reader = (MySqlDataReader)await pullAll.ExecuteReaderAsync();

        while (await reader.ReadAsync()) {
            int id = reader.GetInt32("Id");
            string name = reader.GetString("Name");
            string category = reader.GetString("Category");
            string description = reader.GetString("Description");
            float price = reader.GetFloat("Price");
            DateTime releaseDate = reader.GetDateTime("ReleaseDate");
            bool multiplayer = reader.GetBoolean("Multiplayer");

            Game game = new() {
                Id = id,
                Name = name,
                Category = category,
                Description = description,
                Price = price,
                ReleaseDate = releaseDate,
                Multiplayer = multiplayer
            };

            collection.Add(game);
        }
        
        await reader.CloseAsync();
    }

    public static async Task UpdateDb(int id, Game newData) {
        MySqlCommand command = new("UPDATE Games SET Name = @name, Category = @category, Description = @description, Price = @price, ReleaseDate = @releaseDate, Multiplayer = @multiplayer WHERE id = @id;", Connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@name", newData.Name);
        command.Parameters.AddWithValue("@category", newData.Category);
        command.Parameters.AddWithValue("@description", newData.Description);
        command.Parameters.AddWithValue("@price", newData.Price);
        command.Parameters.AddWithValue("@releaseDate", newData.ReleaseDate);
        command.Parameters.AddWithValue("@multiplayer", newData.Multiplayer);
        
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RemoveFromDb(int id) {
        MySqlCommand command = new($"DELETE FROM Games WHERE Id={id};", Connection);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task RemoveFromDb(string filter) {
        MySqlCommand command = new($"DELETE FROM Games WHERE {filter};", Connection);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task InsertIntoDb(Game newGame) {
        MySqlCommand command = new("INSERT INTO Games (Name, Category, Description, Price, ReleaseDate, Multiplayer) VALUES (@name, @category, @description, @price, @releaseDate, @multiplayer);", Connection);
        command.Parameters.AddWithValue("@name", newGame.Name);
        command.Parameters.AddWithValue("@category", newGame.Category);
        command.Parameters.AddWithValue("@description", newGame.Description);
        command.Parameters.AddWithValue("@price", newGame.Price);
        command.Parameters.AddWithValue("@releaseDate", newGame.ReleaseDate);
        command.Parameters.AddWithValue("@multiplayer", newGame.Multiplayer);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task CloseConnection() {
        if (Connection != null) {
            await Connection.CloseAsync();
        }

        IsConnectedToDatabase = false;
        
        Server = null;
        Username = null;
        Password = null;
        Database = null;
    }

    private static bool ValidateDbName(string dbName) {
        return !string.IsNullOrWhiteSpace(dbName) && dbName.All(char.IsLetterOrDigit);
    }
    
    private static DataSet GetRequiredSchema() {
        DataSet schema = new("LambdaVGames");

        // Create a table
        DataTable table = new("Games");

        // Define columns for the table
        DataColumn idColumn = new("Id", typeof(int));
        idColumn.AutoIncrement = true;

        DataColumn nameColumn = new("Name", typeof(string));
        nameColumn.AllowDBNull = false;
        
        DataColumn categoryColumn = new("Category", typeof(string));
        categoryColumn.AllowDBNull = false;
        
        DataColumn descriptionColumn = new("Description", typeof(string));
        descriptionColumn.AllowDBNull = false;
        
        DataColumn priceColumn = new("Price", typeof(float));
        priceColumn.AllowDBNull = false;
        
        DataColumn releaseDateColumn = new("ReleaseDate", typeof(DateTime));
        releaseDateColumn.AllowDBNull = false;
        
        DataColumn multiplayerColumn = new("Multiplayer", typeof(bool));
        multiplayerColumn.AllowDBNull = false;

        // Add columns to the table
        table.Columns.Add(idColumn);
        table.Columns.Add(nameColumn);
        table.Columns.Add(categoryColumn);
        table.Columns.Add(descriptionColumn);
        table.Columns.Add(priceColumn);
        table.Columns.Add(releaseDateColumn);
        table.Columns.Add(multiplayerColumn);

        // Define a primary key
        table.PrimaryKey = [idColumn];

        // Add the table to the DataSet
        schema.Tables.Add(table);
        
        return schema;
    }

    private static string GetSqlDatatype(Type datatype) {
        if (datatype == typeof(int)) {
            return "int";
        }
        
        if (datatype == typeof(bool)) {
            return "tinyint";
        }

        if (datatype == typeof(float)) {
            return "float";
        }

        if (datatype == typeof(string)) {
            return "text";
        }

        if (datatype == typeof(DateTime)) {
            return "datetime";
        }
        
        throw new NotImplementedException($"Unsupported datatype {datatype}");
    }
}