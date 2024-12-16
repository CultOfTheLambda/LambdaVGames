using System.Text.Json;

namespace LambdaVGames;

public class UserPreferences {
    private static JsonSerializerOptions SerializerOptions { get; } = new() {
        WriteIndented = true
    };
    
    private static JsonSerializerOptions DeserializerOptions { get; } = new() {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };
    
    public static UserPreferences DefaultPreferences { get; } = new() {
        LastConnection = DatabaseConnection.DefaultConnection
    };

    public DatabaseConnection LastConnection { get; set; }

    public string ToJson() {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }

    public static bool FromJson(string json, out UserPreferences? result) {
        try {
            result = JsonSerializer.Deserialize<UserPreferences>(json, DeserializerOptions);
        }
        catch {
            result = null;
            return false;
        }
        
        return result != null;
    }
}