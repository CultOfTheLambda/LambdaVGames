using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Reflection.PortableExecutable;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
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

    // Update object if the box is being edited and update the db
    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Name = NameTextBox.Text;
            UpadateDb();
        }
    }

  /*private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Ca = CategoryTextBox.Text;
        }
    }*/

    private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Description = DescriptionTextBox.Text;
            UpadateDb();

        }
    }

    private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Price = Convert.ToDouble(PriceTextBox.Text);
            UpadateDb();

        }
    }

           
    private void ReleaseDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].ReleaseDate = ReleaseDateTextBox.SelectedDate?? DateTime.MinValue;
            UpadateDb();

        }
    }

    private void YesBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = true;
            UpadateDb();

        }
    }

    private void NoBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = false;
            UpadateDb();

        }
    }

    // Display variables from the object in the boxes
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        NameTextBox.Text = Games[GamesListBox.SelectedIndex].Name;
        //CategoryTextBox.Text = Games[GamesListBox.SelectedIndex].category;
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
    //Update the db with the values from the selected object
    public void UpadateDb()
    {
        MySqlInterop.UpdateDb(Games[GamesListBox.SelectedIndex].Id, Games[GamesListBox.SelectedIndex]);
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