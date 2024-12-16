using System.Windows;
namespace LambdaVGames {
    /// <summary>
    /// Interaktionslogik für AdditionDialogWindow.xaml
    /// </summary>
    public partial class AdditionDialogWindow : Window {
        public Game? newGame;

        public AdditionDialogWindow() {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e) {
            newGame = new Game() {
                Name = NameTextBox.Text,
                Category = CategoryTextBox.Text,
                Description = DescriptionTextBox.Text,
                Price = float.Parse(PriceTextBox.Text),
                ReleaseDate = DateTime.Parse(ReleaseDateTextBox.Text),
                Multiplayer = YesBtn.IsChecked ?? false
            };

            Close();
        }
    }
}
