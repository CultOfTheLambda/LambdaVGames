using System.ComponentModel;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private readonly MySqlConnection connection;
    
    public MainWindow() {
        InitializeComponent();
        
        DatabaseDialog dbDialog = new();
        bool? result = dbDialog.ShowDialog();

        connection = dbDialog.connection ?? throw new NullReferenceException("Database connection is null.");
    }

    protected override void OnClosing(CancelEventArgs e) {
        connection.Close();
        
        base.OnClosing(e);
    }

    private void GamesBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
            NameTextBox.Text = "Name: " + "Test";
            // MultiplayerTextBox.Text = "Multiplayer: " + "Test";
            DescriptionTextBox.Text = "Description: " + "Test";
    }

    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }

    private void MultiplayerTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }

    private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
}

public class A
{
    string name;
    public A(string name)
    {
        this.name = name;
    }

    public override string ToString()
    {
        return name;
    }
}