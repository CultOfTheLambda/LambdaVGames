using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Reflection.PortableExecutable;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private MySqlConnection connection;
    public ObservableCollection<Game> Games { get; } = [];

    ObservableCollection<Game> FilteredGames = new ObservableCollection<Game>(); //for search bar

    private bool ignoreSelectedIndexChanged = false;

    public MainWindow() {
        InitializeComponent();

        DataContext = this;

        DatabaseDialog dbDialog = new();
        dbDialog.ShowDialog();
        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");

        foreach(var game in Games)
    {
            FilteredGames.Add(game);
        }

        GamesListBox.ItemsSource = FilteredGames;
    }

    protected override void OnClosing(CancelEventArgs e) {
        MySqlInterop.CloseConnection();


        base.OnClosing(e);
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
        await RefreshGamesList();
    }

    // Update object if the box is being edited
    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Name = NameTextBox.Text;
        }
    }

    private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Category = CategoryTextBox.Text;
        }
    }

    private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Description = DescriptionTextBox.Text;
        }
    }

    private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Price = Convert.ToSingle(PriceTextBox.Text);
        }
    }

           
    private void ReleaseDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].ReleaseDate = ReleaseDateTextBox.SelectedDate?? DateTime.MinValue;
        }
    }

    private void YesBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = true;
        }
    }

    private void NoBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = false;
        }
    }

    // Display variables from the object in the boxes
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (ignoreSelectedIndexChanged)
        {
            return;
        }

        if (GamesListBox.SelectedItem != null)
        {
            var selectedGame = GamesListBox.SelectedItem as Game;

            // Zeige die Details des ausgewählten Spiels an
            NameTextBox.Text = selectedGame.Name;
            CategoryTextBox.Text = selectedGame.Category;
            DescriptionTextBox.Text = selectedGame.Description;
            PriceTextBox.Text = selectedGame.Price.ToString();
            ReleaseDateTextBox.Text = selectedGame.ReleaseDate.ToString();

            switch (selectedGame.Multiplayer)
            {
                case true:
                    YesBtn.IsChecked = true;
                    break;

                case false:
                    NoBtn.IsChecked = true;
                    break;
            }
            if (GamesListBox.SelectedItem != null)
            {
                var selectedGame1 = GamesListBox.SelectedItem as Game;
                MessageBox.Show($"Selected Game: {selectedGame1.Name}");
            }
        }
    }

    private void GamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ignoreSelectedIndexChanged)
        {
            return;
        }
        else
        {
            if (GamesListBox.SelectedIndex >= 0 && GamesListBox.SelectedIndex < FilteredGames.Count)
            {
                var selectedGame = FilteredGames[GamesListBox.SelectedIndex];

                NameTextBox.Text = selectedGame.Name;
                CategoryTextBox.Text = selectedGame.Category;
                DescriptionTextBox.Text = selectedGame.Description;
                PriceTextBox.Text = selectedGame.Price.ToString();
                ReleaseDateTextBox.Text = selectedGame.ReleaseDate.ToString();

                switch (selectedGame.Multiplayer)
                {
                    case true:
                        YesBtn.IsChecked = true;
                        break;

                    case false:
                        NoBtn.IsChecked = true;
                        break;
                }
                ReleaseDateTextBox.SelectedDate = selectedGame.ReleaseDate;
            }
        }

        
    }

    private async void MenuItem_OnClick(object sender, RoutedEventArgs e) {
        DatabaseDialog dbDialog = new(MySqlInterop.Server?? "localhost", MySqlInterop.Username?? string.Empty, string.Empty, MySqlInterop.Database?? "myDb");
        dbDialog.ShowDialog();

        connection = MySqlInterop.Connection ?? connection;

        await RefreshGamesList();
    }

    private async Task RefreshGamesList() {
        Games.Clear();

        MySqlCommand pullAll = new("SELECT * FROM Games", connection);

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

            Games.Add(game);
        }
    }

    private void OnMenuLinkClick(object sender, RoutedEventArgs e) {
        Process.Start(new ProcessStartInfo {
            FileName = ((MenuItem)sender).Tag.ToString(),
            UseShellExecute = true
        });
    }

    private void OnExitClick(object sender, RoutedEventArgs e) {
        this.Close();
    }

    private void OnPreferenceMenuLink(object sender, RoutedEventArgs e) {
        PreferencesDialog pD = new();
        pD.ShowDialog();
    }

    private void searchBartxtbx_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (searchBarTxtbx.Text != "")
        {
            searchBarLbl.Visibility = Visibility.Collapsed;
        }
        else
        {
            searchBarLbl.Visibility = Visibility.Visible;
        }

        string searchText = searchBarTxtbx.Text.ToLower();
        ignoreSelectedIndexChanged = true;

        var filtered = Games.Where(game => game.Name.ToLower().StartsWith(searchText)).Distinct().ToList();

        FilteredGames.Clear();
        foreach (var game in filtered)
        {
            FilteredGames.Add(game);
        }

        GamesListBox.ItemsSource = null;
        GamesListBox.ItemsSource = FilteredGames;

        if (GamesListBox.SelectedItem != null)
        {
            var selectedGame = GamesListBox.SelectedItem as Game;

            var selectedIndexInFiltered = FilteredGames.IndexOf(selectedGame);

            if (selectedIndexInFiltered >= 0)
            {
                GamesListBox.SelectedIndex = selectedIndexInFiltered;
            }
            else
            {
                GamesListBox.SelectedIndex = -1;
            }
        }
        else
        {
            GamesListBox.SelectedIndex = -1;
        }

        ignoreSelectedIndexChanged = false;
    }
}