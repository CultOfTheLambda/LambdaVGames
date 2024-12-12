using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private MySqlConnection connection;

    private readonly List<Game> databaseCollection = [];

    public ObservableCollection<Game> Games { get; } = [];

    private string filter = string.Empty;

    public MainWindow() {
        InitializeComponent();

        DataContext = this;

        DatabaseDialog dbDialog = new();
        dbDialog.ShowDialog();
        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");

        GamesListBox.ItemsSource = Games;
    }

    protected override void OnClosing(CancelEventArgs e) {
        MySqlInterop.CloseConnection();


        base.OnClosing(e);
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
        await RefreshGamesList(true);
    }

    // Update object if the box is being edited
    private async void NameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Name = NameTextBox.Text;
            UpadateDb();
        }
    }

    private async void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Category = CategoryTextBox.Text;
        }
    }

    private async void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Description = DescriptionTextBox.Text;
            UpadateDb();

        }
    }

    private async void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            if (float.TryParse(PriceTextBox.Text, out float result)) {
                Games[GamesListBox.SelectedIndex].Price = result;
                UpadateDb();
            }
            else
            {
                PriceTextBox.Text = Games[GamesListBox.SelectedIndex].Price.ToString();
                MessageBox.Show("Invalid price");
            }
        }
    }

           
    private async void ReleaseDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].ReleaseDate = ReleaseDateTextBox.SelectedDate?? DateTime.MinValue;
            UpadateDb();

        }
    }

    private async void YesBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = true;
            UpadateDb();

        }
    }

    private async void NoBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = false;
            UpadateDb();

        }
    }

    // Display variables from the object in the boxes
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (GamesListBox.SelectedIndex > -1) {
            Game selectedGame = Games[GamesListBox.SelectedIndex];

            // Zeige die Details des ausgewählten Spiels an
            NameTextBox.Text = selectedGame.Name;
            CategoryTextBox.Text = selectedGame.Category;
            DescriptionTextBox.Text = selectedGame.Description;
            PriceTextBox.Text = selectedGame.Price.ToString();
            ReleaseDateTextBox.Text = selectedGame.ReleaseDate.ToString();

            switch (selectedGame.Multiplayer) {
                case true:
                    YesBtn.IsChecked = true;
                    break;

                case false:
                    NoBtn.IsChecked = true;
                    break;
            }
        }
    }

    //Update the db with the values from the selected object
    public void UpadateDb()
    {
        MySqlInterop.UpdateDb(Games[GamesListBox.SelectedIndex].Id, Games[GamesListBox.SelectedIndex]);
    }

    private async void MenuItem_OnClick(object sender, RoutedEventArgs e) {
        DatabaseDialog dbDialog = new(MySqlInterop.Server?? "localhost", MySqlInterop.Username?? string.Empty, string.Empty, MySqlInterop.Database?? "myDb");
        dbDialog.ShowDialog();

        connection = MySqlInterop.Connection ?? connection;

        await RefreshGamesList(true);
    }

    private async Task RefreshGamesList(bool queryDatabase = true) {
        if (queryDatabase) {
            await MySqlInterop.QueryDatabase(databaseCollection);
        }

        Games.Clear();

        foreach(Game game in databaseCollection) {
            if (game.Name.ToLower().StartsWith(filter.ToLower())) {
                Games.Add(game);
            }
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

    private async void SearchBartxtbx_TextChanged(object sender, TextChangedEventArgs e) {
        if (SearchBarTxtbx.Text != string.Empty) {
            searchBarLbl.Visibility = Visibility.Collapsed;
        }
        else {
            searchBarLbl.Visibility = Visibility.Visible;
        }

        filter = SearchBarTxtbx.Text;

        await RefreshGamesList(false);
    }
}