using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MySqlConnection connection;

    private List<Game> data = [];

    public MainWindow()
    {
        InitializeComponent();
        DatabaseDialog dbDialog = new();
        bool? result = dbDialog.ShowDialog();

        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");

        data.Add(new Game
        {
            id = 1,
            name = "Game 1",
            category = "Action",
            description = "A fast-paced action game.",
            price = 29.99,
            date = new DateTime(2024, 12, 1),
            multiplayer = true
        });

        data.Add(new Game
        {
            id = 2,
            name = "Game 2",
            category = "Action",
            description = "A fast-paced action game.",
            price = 29.99,
            date = new DateTime(2024, 12, 1),
            multiplayer = true
        });

        GamesBox.ItemsSource = data;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        connection.Close();

        base.OnClosing(e);
    }


    private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].name = NameTextBox.Text;
        }
        else;
    }

    private void CategoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].category = CategoryTextBox.Text;
        }
        else;
    }

    private void DescriptionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].description = DescriptionTextBox.Text;
        }
        else;
    }

    private void PriceTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].price = Convert.ToDouble(PriceTextBox.Text);
        }
        else;

    }

    private void ReleaseDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].date = ReleaseDateTextBox.SelectedDate;
        }
        else;
    }
    private void JaBtn_Click(object sender, RoutedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].multiplayer = true;
        }
        else;
    }
    private void NeinBtn_Click(object sender, RoutedEventArgs e)
    {
        if (GamesBox.SelectedIndex >= 0)
        {
            data[GamesBox.SelectedIndex].multiplayer = false;
        }
        else;
    }


    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NameTextBox.Text = data[GamesBox.SelectedIndex].name;
        CategoryTextBox.Text = data[GamesBox.SelectedIndex].category;
        DescriptionTextBox.Text = data[GamesBox.SelectedIndex].description;
        PriceTextBox.Text = data[GamesBox.SelectedIndex].price.ToString();
        ReleaseDateTextBox.Text = data[GamesBox.SelectedIndex].date.ToString();
        switch (data[GamesBox.SelectedIndex].multiplayer)
        {
            case true:
                JaBtn.IsChecked = true;
                break;
            case false:
                NeinBtn.IsChecked = true;
                break;
        }
    }
}