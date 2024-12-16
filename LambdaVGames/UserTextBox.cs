using System.Windows.Controls;

namespace LambdaVGames;

public class UserTextBox : TextBox {
    protected bool suppressTextChanged;
    
    /// <summary>
    /// Change the text of this textbox.
    /// </summary>
    /// <param name="text">The new text of the textbox.</param>
    /// <param name="suppressTextChanged">Whether the <see cref="OnTextChanged"/> event should be suppressed.</param>
    public void SetText(string text, bool suppressTextChanged = true) {
        this.suppressTextChanged = suppressTextChanged;
        
        this.Text = text;
    }
    
    protected override void OnTextChanged(TextChangedEventArgs e) {
        // Raise the event if it is not suppressed.
        if (!suppressTextChanged) {
            base.OnTextChanged(e);   
        }

        // Reset the suppress-flag.
        suppressTextChanged = false;
    }
}