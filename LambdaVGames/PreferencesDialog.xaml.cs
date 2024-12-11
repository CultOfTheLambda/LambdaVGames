using System.Windows;

namespace LambdaVGames;

public partial class PreferencesDialog : Window{
    public PreferencesDialog() {
        InitializeComponent();
    }

    private void OnCancleClick(object sender, RoutedEventArgs e) {
        this.Close();
    }
}