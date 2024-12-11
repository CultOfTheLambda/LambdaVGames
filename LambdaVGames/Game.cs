namespace LambdaVGames;

public class Game {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool Multiplayer { get; set; }

    public override string ToString() {
        return Name;
    }
}