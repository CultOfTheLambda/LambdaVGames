using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LambdaVGames;

public partial class PreferencesDialog : Window, INotifyPropertyChanged{
    private string _settingsPage = "../SettingsPages/AccountPage.xaml";
    public string SettingsPage {
        get => _settingsPage;
        set {
            if (_settingsPage != value) {
                _settingsPage = value;
                OnPropertyChanged();
            }
        }
    }

    public PreferencesDialog() {
        InitializeComponent();
        this.DataContext = this;
    }

    private void OnTreePointSelect(object sender, RoutedEventArgs e) {
        TreeViewItem item = sender as TreeViewItem;
        SettingsPage = $"../SettingsPages/{item.Tag.ToString()}Page.xaml";
    }

    private void OnCancleClick(object sender, RoutedEventArgs e) {
        this.Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}