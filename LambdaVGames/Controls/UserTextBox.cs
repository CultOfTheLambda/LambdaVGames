using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LambdaVGames.Controls;

/// <summary>
/// A version of <see cref="TextBox"/> that allows more precise control over TextEvents.
/// </summary>
public class UserTextBox : TextBox {
    public static readonly RoutedEvent TextEditEndEvent = EventManager.RegisterRoutedEvent("TextEditEnd", RoutingStrategy.Bubble,
        typeof(TextEditEndEventHandler), typeof(UserTextBox));
    
    /// <summary>
    /// Fired when the user finishes editing the <see cref="UserTextBox"/>, by pressing Enter or removing focus.
    /// </summary>
    /// <param name="e"></param>
    public event TextEditEndEventHandler TextEditEnd {
        add => AddHandler(TextEditEndEvent, value);

        remove => RemoveHandler(TextEditEndEvent, value);
    }
    
    protected bool suppressTextChanged;
    
    /// <summary>
    /// Change the text of this textbox.
    /// </summary>
    /// <param name="text">The new text of the textbox.</param>
    /// <param name="suppressTextChanged">Whether the <see cref="OnTextChanged"/> event should be suppressed.</param>
    public void SetText(string text, bool suppressTextChanged = true) {
        this.suppressTextChanged = suppressTextChanged;
        
        Text = text;
    }
    
    protected override void OnTextChanged(TextChangedEventArgs e) {
        // Raise the event if it is not suppressed.
        if (!suppressTextChanged) {
            base.OnTextChanged(e);   
        }
        
        e.Handled = true;
        
        // Reset the suppress-flag.
        suppressTextChanged = false;
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        if (e.Key == Key.Enter) {
            e.Handled = true;
            
            OnTextEditEnd(new TextEditEndEventArgs(TextEditEndEvent, UndoAction.None));
        }
        
        base.OnKeyDown(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e) {
        OnTextEditEnd(new TextEditEndEventArgs(TextEditEndEvent, UndoAction.None));
        
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Fired when the user finishes editing the <see cref="UserTextBox"/>, by pressing Enter or removing focus.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnTextEditEnd(TextEditEndEventArgs e) {
        RaiseEvent(e);
    }
}