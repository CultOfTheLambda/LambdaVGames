using System.Windows;
using LambdaVGames.Classes;

namespace LambdaVGames.Windows.Dialogs;

/// <summary>
/// A dialog window to connect to a database.
/// </summary>
public partial class DatabaseDialog : Window {
    private UserPreferences preferences;
    
    public DatabaseDialog() {
        InitializeComponent();
    }

    public DatabaseDialog(UserPreferences preferences) {
        InitializeComponent();
        
        this.preferences = preferences;
        
        HostTextBox.Text = preferences.LastConnection.Server?? string.Empty;
        UserTextBox.Text = preferences.LastConnection.Username ?? string.Empty;
        PasswordTextBox.Password = preferences.LastConnection.Password ?? string.Empty;
        DatabaseTextBox.Text = preferences.LastConnection.Database?? string.Empty;
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e) {
        try {
            LoadingIcon.Visibility = Visibility.Visible;

            await MySqlInterop.ConnectToDatabaseServer(HostTextBox.Text, UserTextBox.Text,
                PasswordTextBox.Password);
            
            LoadingIcon.Visibility = Visibility.Hidden;
        }
        catch (Exception ex) {
            LoadingIcon.Visibility = Visibility.Hidden;
            
            MessageBox.Show($"Unable to connect to the database server: {ex.Message}", "Connection failed",
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            
            PasswordTextBox.Clear();
        }

        preferences.LastConnection = new() {
            Server = HostTextBox.Text,
            Username = UserTextBox.Text,
            Password = PasswordTextBox.Password,
            Database = DatabaseTextBox.Text
        };
        

        if (MySqlInterop.IsConnectedToServer) {
            if (string.IsNullOrWhiteSpace(DatabaseTextBox.Text)) {
                if (MessageBox.Show($"Would you like to to create a default database ({MySqlInterop.DefaultDbname})?",
                        "Invalid Database", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
                    await MySqlInterop.CreateDatabaseAndConnect();
                    
                    Close();
                }
            }
            else {
                if (!await MySqlInterop.TryConnectToDatabase(DatabaseTextBox.Text)) {
                    if (MessageBox.Show("The specified database does not exist. Do you want to create it?",
                            "Invalid Database", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
                        await MySqlInterop.CreateDatabaseAndConnect(DatabaseTextBox.Text);
                        
                        Close();
                    }
                }
                else {
                    if (!await MySqlInterop.ValidateDbSchema()) {
                        if (MessageBox.Show("The schema of the specified database is invalid. Would you like to recreate it?\nAttention:\n" +
                                "This will delete all data previously stored in the database. This action cannot be undone.",
                                "Invalid db schema", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
                            await MySqlInterop.CreateDbSchema();
                            Close();
                        }
                    }
                    else {
                        Close();   
                    }
                }
            }
        }
    }
}