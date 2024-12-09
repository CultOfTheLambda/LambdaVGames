using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace LambdaVGames;

/// <summary>
/// A dialog window to connect to a database.
/// </summary>
public partial class DatabaseDialog : Window {
    public MySqlConnection? connection;
    
    public DatabaseDialog() {
        InitializeComponent();
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e) {
        string connectionString = string.Empty;

        connectionString += $"server={HostTextBox.Text};";
        connectionString += $"user id={UserTextBox.Text};";
        connectionString += $"password={PasswordTextBox.Password};";
        connectionString += $"database={DatabaseTextBox.Text}";

        connection = new MySqlConnection(connectionString);

        try {
            LoadingIcon.Visibility = Visibility.Visible;
            
            await connection.OpenAsync();
            
            LoadingIcon.Visibility = Visibility.Hidden;
        }
        catch (Exception ex) {
            LoadingIcon.Visibility = Visibility.Hidden;
            
            MessageBox.Show($"Unable to connect to the database: {ex.Message}", "Connection failed",
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        if (connection.State == ConnectionState.Open) {
            Close();
        }
    }
}