using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MySql.Data.MySqlClient;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private readonly MySqlConnection connection;

    public ObservableCollection<Game> Games { get; } = [];
    
    public MainWindow() {
        InitializeComponent();
        
        DatabaseDialog dbDialog = new();
        dbDialog.ShowDialog();

        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");
        
        DataContext = this;
    }

    protected override void OnClosing(CancelEventArgs e) {
        MySqlInterop.CloseConnection();
        
        base.OnClosing(e);
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
        await RefreshGamesList();
    }

    private async Task RefreshGamesList() {
        Games.Clear();
        
        MySqlCommand pullAll = new("SELECT * FROM Games", connection);

        await using MySqlDataReader reader = (MySqlDataReader)await pullAll.ExecuteReaderAsync();

        while (await reader.ReadAsync()) {
            int id = reader.GetInt32("Id");
            string name = reader.GetString("Name");
            string description = reader.GetString("Description");
            double price = reader.GetDouble("Price");
            DateTime releaseDate = reader.GetDateTime("ReleaseDate");
            bool multiplayer = reader.GetBoolean("Multiplayer");

            Game game = new() {
                Id = id,
                Name = name,
                Description = description,
                Price = price,
                ReleaseDate = releaseDate,
                Multiplayer = multiplayer
            };

            Games.Add(game);
        }
    }
}