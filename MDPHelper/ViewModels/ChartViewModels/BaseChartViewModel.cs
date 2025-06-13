using System.ComponentModel;
using SkiaSharp;

namespace fyp_MDPHelperApp.ViewModels.ChartViewModels;

public abstract class BaseChartViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected const int MAX_DATA_POINT = 1000;
    
    // Method to trigger property change notification
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Method to get the color based on the app theme
    protected SKColor GetValueLabelColor()
    {
        return Application.Current.RequestedTheme == AppTheme.Dark
            ? SKColors.White
            : SKColors.Black;
    }

    public abstract void UpdateEntriesValueLabelColor();
}