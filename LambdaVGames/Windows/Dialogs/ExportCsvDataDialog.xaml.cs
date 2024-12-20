using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames.Windows.Dialogs;

/// <summary>
/// A dialog window to connect to a database.
/// </summary>
public partial class ExportCsvDataDialog : Window {
    private static readonly char[] separators = [';', ',', '\t', '|', ':', '~', ' ', '^'];
    
    public char Separator { get; private set; }
    public bool AddHeader { get; private set; }
    
    public ExportCsvDataDialog() {
        InitializeComponent();
    }

    private void ExportButton_OnClick(object sender, RoutedEventArgs e) {
        Separator = separators[SeparatorComboBox.SelectedIndex];
        AddHeader = HeaderCheckbox.IsChecked?? false;
        
        Close();
    }
}