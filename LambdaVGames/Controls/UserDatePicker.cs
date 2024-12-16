using System.Windows.Controls;

namespace LambdaVGames.Controls;

/// <summary>
/// A version of <see cref="DatePicker"/> that allows more precise control over the SelectedDateChanged event.
/// </summary>
public class UserDatePicker : DatePicker {
    protected bool suppressSelectedDateChanged;
    protected bool suppressSecond;
    
    public void SetDate(DateTime? date, bool suppressSelectedDateChanged = true) {
        this.suppressSelectedDateChanged = suppressSelectedDateChanged;
        
        SelectedDate = date;
    }

    protected override void OnSelectedDateChanged(SelectionChangedEventArgs e) {
        // Raise the event if it is not suppressed.
        if (!suppressSelectedDateChanged && !suppressSecond) {
            base.OnSelectedDateChanged(e);   
        }
        else {
            e.Handled = true;

            suppressSecond = suppressSelectedDateChanged && !suppressSecond;
        }
        
        // Reset the suppress-flag.
        suppressSelectedDateChanged = false;
    }
}