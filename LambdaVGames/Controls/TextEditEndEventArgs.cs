using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames.Controls;

public class TextEditEndEventArgs : RoutedEventArgs {
    public UndoAction UndoAction { get; }
    
    public TextEditEndEventArgs(RoutedEvent id, UndoAction action) {
        if (action is < UndoAction.None or > UndoAction.Create) {
            throw new InvalidEnumArgumentException("action", (int)action, typeof(UndoAction));
        }
        
        RoutedEvent = id ?? throw new ArgumentNullException(nameof(id));
        UndoAction = action;
    }
        
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget) {
        TextEditEndEventHandler handler = (TextEditEndEventHandler)genericHandler;
        handler(genericTarget, this);
    }
}