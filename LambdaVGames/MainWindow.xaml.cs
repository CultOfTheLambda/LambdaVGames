using System.ComponentModel;
using System.Windows;
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
}