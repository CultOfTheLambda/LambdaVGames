using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using LambdaVGames.Classes;
using LambdaVGames.Controls;
using LambdaVGames.Windows.Dialogs;

namespace LambdaVGames.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public const string LVG_LOCAL_PATH = "lvg_local";
    public const string LVG_CONFIG_PATH = $"{LVG_LOCAL_PATH}/lvg_config.json";
    public const string LVG_IMAGES_PATH = $"{LVG_LOCAL_PATH}/images";
    
    private UserPreferences preferences;
    
    private MySqlConnection connection;

    private readonly List<Game> databaseCollection = [];

    public ObservableCollection<Game> Games { get; } = [];
    
    private Game? selectedGame;

    private string filter = string.Empty;

    public MainWindow() {
        InitializeComponent();

        DataContext = this;

        GamesListBox.ItemsSource = Games;
    }

    protected override async void OnClosing(CancelEventArgs e) {
        await MySqlInterop.CloseConnection();


        base.OnClosing(e);
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
        DatabaseDialog dbDialog = new(preferences);
        dbDialog.ShowDialog();
        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");
        
        SavePreferences();
        
        await RefreshGamesList();
        
        SetDataControlEnabled(false);
    }
    
    private void MainWindow_Initialized(object? sender, EventArgs e) {
        if (!Directory.Exists(LVG_LOCAL_PATH)) {
            Directory.CreateDirectory(LVG_LOCAL_PATH);
        }

        if (!Directory.Exists(LVG_IMAGES_PATH)) {
            Directory.CreateDirectory(LVG_IMAGES_PATH);
        }
        
        if (!File.Exists(LVG_CONFIG_PATH)) {
            File.WriteAllText(LVG_CONFIG_PATH, UserPreferences.DefaultPreferences.ToJson());
            preferences = UserPreferences.DefaultPreferences;
        }
        else {
            if (UserPreferences.FromJson(File.ReadAllText(LVG_CONFIG_PATH), out UserPreferences? preferences)) {
                this.preferences = preferences!;
            }
            else {
                this.preferences = UserPreferences.DefaultPreferences;
                
                if (MessageBox.Show(
                        "The config file is invalid. Default Preferences will be used. Would you like to regenerate the config file?",
                        "Invalid config file.", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
                    SavePreferences();
                }
            }
        }
    }

    private void SavePreferences() {
        File.WriteAllText(LVG_CONFIG_PATH, preferences.ToJson());
    }
    
    private async void NameTextBox_TextEditEnd(object sender, TextEditEndEventArgs e) {
        if (selectedGame != null) {
            selectedGame.Name = NameTextBox.Text;
            await UpdateDb();

            await RefreshGamesList(false, true);
        }
    }

    private async void CategoryTextBox_TextEditEnd(object sender, TextEditEndEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Category = CategoryTextBox.Text;
            await UpdateDb();
        }
    }

    private async void DescriptionTextBox_TextEditEnd(object sender, TextEditEndEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Description = DescriptionTextBox.Text;
            await UpdateDb();

        }
    }

    private async void PriceTextBox_TextEditEnd(object sender, TextEditEndEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            if (float.TryParse(PriceTextBox.Text, out float result)) {
                Games[GamesListBox.SelectedIndex].Price = result;
                await UpdateDb();
            }
            else {
                PriceTextBox.Text = Games[GamesListBox.SelectedIndex].Price.ToString();
                MessageBox.Show("Invalid price");
            }
        }
    }
           
    private async void ReleaseDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].ReleaseDate = ReleaseDateTextBox.SelectedDate?? DateTime.MinValue;
            await UpdateDb();
        }
    }

    private async void YesBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = true;
            await UpdateDb();
        }
    }

    private async void NoBtn_Click(object sender, RoutedEventArgs e) {
        if (GamesListBox.SelectedIndex >= 0) {
            Games[GamesListBox.SelectedIndex].Multiplayer = false;
            await UpdateDb();
        }
    }
    
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        // Skip if the selected game did not change
        if (GamesListBox.SelectedIndex > -1 && selectedGame != Games[GamesListBox.SelectedIndex]) {
            selectedGame = Games[GamesListBox.SelectedIndex];
            
            // Show details of the selected game
            NameTextBox.SetText(selectedGame.Name);
            CategoryTextBox.SetText(selectedGame.Category);
            DescriptionTextBox.SetText(selectedGame.Description);
            PriceTextBox.SetText(selectedGame.Price.ToString(CultureInfo.CurrentCulture));
            ReleaseDateTextBox.SetDate(selectedGame.ReleaseDate);
            GameImageControl.ImageId = selectedGame.Id;
            
            switch (selectedGame.Multiplayer) {
                case true:
                    YesBtn.IsChecked = true;
                    break;

                case false:
                    NoBtn.IsChecked = true;
                    break;
            }
            
            SetDataControlEnabled(true);
        }
        else {
            selectedGame = null;
            
            NameTextBox.SetText(string.Empty);
            CategoryTextBox.SetText(string.Empty);
            DescriptionTextBox.SetText(string.Empty);
            PriceTextBox.SetText(string.Empty);
            ReleaseDateTextBox.SetDate(null);
            GameImageControl.ImageId = -1;
            
            YesBtn.IsChecked = false;
            NoBtn.IsChecked = false;
            
            SetDataControlEnabled(false);
        }
    }

    private void SetDataControlEnabled(bool enabled) {
        NameTextBox.IsEnabled = enabled;
        CategoryTextBox.IsEnabled = enabled;
        DescriptionTextBox.IsEnabled = enabled;
        PriceTextBox.IsEnabled = enabled;
        ReleaseDateTextBox.IsEnabled = enabled;
        GameImageControl.IsEnabled = enabled;
            
        YesBtn.IsEnabled = enabled;
        NoBtn.IsEnabled = enabled;
    }

    //Update the db with the values from the selected object
    private async Task UpdateDb() {
        try {
            await MySqlInterop.UpdateDb(Games[GamesListBox.SelectedIndex].Id, Games[GamesListBox.SelectedIndex]);
        }
        catch (Exception e) {
            MessageBox.Show($"An error occured: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void MenuItem_OnClick(object sender, RoutedEventArgs e) {
        DatabaseDialog dbDialog = new(preferences);
        dbDialog.ShowDialog();

        connection = MySqlInterop.Connection ?? connection;
        
        SavePreferences();

        await RefreshGamesList(true);
    }

    private async Task RefreshGamesList(bool queryDatabase = true, bool preserveSelection = false) {
        int selection = GamesListBox.SelectedIndex;
        
        if (queryDatabase) {
            await MySqlInterop.QueryDatabase(databaseCollection);
        }

        Games.Clear();

        foreach(Game game in databaseCollection) {
            if (game.Name.ToLower().StartsWith(filter.ToLower())) {
                Games.Add(game);
            }
        }

        if (preserveSelection && Games.Count > selection) {
            GamesListBox.SelectedIndex = selection;
        }
    }

    private void OnMenuLinkClick(object sender, RoutedEventArgs e) {
        Process.Start(new ProcessStartInfo {
            FileName = ((MenuItem)sender).Tag.ToString(),
            UseShellExecute = true
        });
    }

    private void OnExitClick(object sender, RoutedEventArgs e) {
        this.Close();
    }

    private void OnPreferenceMenuLink(object sender, RoutedEventArgs e) {
        PreferencesDialog pD = new();
        pD.ShowDialog();
    }

    private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e) {
        if (SearchBar.Text != string.Empty) {
            searchBarLbl.Visibility = Visibility.Collapsed;
        }
        else {
            searchBarLbl.Visibility = Visibility.Visible;
        }

        filter = SearchBar.Text;

        await RefreshGamesList(false);
    }

    private async void RemoveBtn_Click(object sender, RoutedEventArgs e) {
        if (selectedGame != null) {
            await MySqlInterop.RemoveFromDb(selectedGame.Id);
            
            ImageControl.CleanupImage(selectedGame.Id);

            await RefreshGamesList(true);
        }
    }

    private async void AddObjBtn_Click(object sender, RoutedEventArgs e) {
        AdditionDialogWindow additionDialog = new();

        additionDialog.ShowDialog();

        if (additionDialog.newGame != null) {
            await MySqlInterop.InsertIntoDb(additionDialog.newGame);
        }

        await RefreshGamesList(true);
    } 
}