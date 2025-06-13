using System.ComponentModel;

namespace fyp_MDPHelperApp.ViewModels;

public class ThemeViewModel
{
    private bool _isDarkTheme;

    public ThemeViewModel()
    {
        // Initialize the theme based on the current app theme
        if (Application.Current != null) IsDarkTheme = Application.Current.RequestedTheme == AppTheme.Dark;
    }

    public static ThemeViewModel Instance { get; } = new();

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme != value)
            {
                _isDarkTheme = value;
                OnPropertyChanged(nameof(IsDarkTheme));
                SetAppTheme();
            }
        }
    }

    private void SetAppTheme()
    {
        if (Application.Current != null)
            Application.Current.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}