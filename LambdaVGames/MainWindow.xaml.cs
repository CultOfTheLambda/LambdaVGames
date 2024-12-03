using System.ComponentModel;
using System.Windows;
using MySql.Data.MySqlClient;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private static MySqlConnection connection;
    private const string dbConnection = "server=localhost;user id=Test;password=Test;database=LambdaVGamesDb";
    
    public MainWindow() {
        InitializeComponent();

        using (connection = new MySqlConnection(dbConnection)) {
            connection.Open();

            if (connection.State == System.Data.ConnectionState.Open) {
                MessageBox.Show("Connection established.");
            }
            else {
                MessageBox.Show("Unable to connect to database.");
            }
        }
    }

    protected override void OnClosing(CancelEventArgs e) {
        connection.Close();
        
        base.OnClosing(e);
    }
}