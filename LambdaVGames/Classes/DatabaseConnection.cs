namespace LambdaVGames.Classes;

public class DatabaseConnection {
    public static DatabaseConnection DefaultConnection { get; } = new() {
        Name = "Default",
        Server = "localhost",
        Username = "root",
        Password = null,
        Database = "LambdaVGames"
    };
    
    public string? Name { get; init; }
    public string? Server { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? Database { get; init; }
}