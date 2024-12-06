using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private static MySqlConnection connection;
    private const string dbConnection = "server=localhost;user id=Test;password=Test;database=LambdaVGamesDb";

    private List<Game> data = [];

    public MainWindow()
    {
        InitializeComponent();
        DatabaseDialog dbDialog = new();
        bool? result = dbDialog.ShowDialog();

        connection = dbDialog.connection ?? throw new NullReferenceException("Database connection is null.");

            

        data.Add(new A("hi"));

        GamesBox.ItemsSource = data;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        connection.Close();

        base.OnClosing(e);
    }


    private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }

    private void CategoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }

    private void DescriptionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }

    private void PriceTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }


    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NameTextBox.Text = data[GamesBox.SelectedIndex].name;
        CategoryTextBox.Text = data[GamesBox.SelectedIndex].category;
        DescriptionTextBox.Text = data[GamesBox.SelectedIndex].description;
        PriceTextBox.Text = data[GamesBox.SelectedIndex].price.ToString();
        ReleaseDateTextBox.Text = data[GamesBox.SelectedIndex].date.ToString();
        switch(data[GamesBox.SelectedIndex].multiplayer)
        {
            case true:
                JaBtn.IsChecked = true;
                NeinBtn.IsChecked = false;
                break;
            case false:
                JaBtn.IsChecked = false;
                NeinBtn.IsChecked = true;
                break;
        }
    }
}