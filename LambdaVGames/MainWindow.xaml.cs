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

    public MainWindow() {
        InitializeComponent();
        
        DataContext = this;
        
        DatabaseDialog dbDialog = new();
        dbDialog.ShowDialog();
        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");
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
        NameTextBox.Text = Games[GamesListBox.SelectedIndex].Name;
        CategoryTextBox.Text = Games[GamesListBox.SelectedIndex].Category;
        DescriptionTextBox.Text = Games[GamesListBox.SelectedIndex].Description;
        PriceTextBox.Text = Games[GamesListBox.SelectedIndex].Price.ToString();
        ReleaseDateTextBox.Text = Games[GamesListBox.SelectedIndex].ReleaseDate.ToString();

        switch (Games[GamesListBox.SelectedIndex].Multiplayer) {
            case true:
                YesBtn.IsChecked = true;
                break;

            case false:
                NoBtn.IsChecked = true;
                break;
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
}