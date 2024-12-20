using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames.Windows.Dialogs;

/// <summary>
/// A dialog window to connect to a database.
/// </summary>
public partial class ImportCsvDataDialog : Window {
    private static readonly char[] separators = [';', ',', '\t', '|', ':', '~', ' ', '^'];
    
    public char Separator { get; private set; }
    public bool IgnoreHeader { get; private set; }
    
    public ImportCsvDataDialog() {
        InitializeComponent();
    }

    private void ImportButton_OnClick(object sender, RoutedEventArgs e) {
        Separator = separators[SeparatorComboBox.SelectedIndex];
        IgnoreHeader = IgnoreHeaderCheckbox.IsChecked?? false;
        
        Close();
    }
}