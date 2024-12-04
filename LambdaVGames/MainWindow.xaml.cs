using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    // Deleted read-only from this variable because someone should be able to change the Database after Application start.
    private MySqlConnection connection;
    
    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e) {
        base.OnInitialized(e);
        
        /*
        if (connection is null || connection.State == ConnectionState.Closed) {
            // Can be made into a methode later on
            DatabaseDialog dbDialog = new();
            bool? result = dbDialog.ShowDialog();

            connection = dbDialog.connection ?? throw new NullReferenceException("Database connection is null.");
        }
        */
    }

    protected override void OnClosing(CancelEventArgs e) {
        connection.Close();
        
        base.OnClosing(e);
    }
}