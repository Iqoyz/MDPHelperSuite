using System.Collections.ObjectModel;
using System.ComponentModel;

namespace fyp_MDPHelperApp.ViewModels;

public class LogViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Log> _logMessages;

    public LogViewModel()
    {
        LogMessages = new ObservableCollection<Log>();
    }

    public static LogViewModel Instance { get; } = new();

    public ObservableCollection<Log> LogMessages
    {
        get => _logMessages;
        set
        {
            if (_logMessages != value)
            {
                _logMessages = value;
                OnPropertyChanged(nameof(LogMessages));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Log : INotifyPropertyChanged
{
    private Color _messageColor;

    public Log()
    {
        // Default color based on theme
        MessageColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
    }

    public string LogMessage { get; set; }

    public Color MessageColor
    {
        get => _messageColor;
        set
        {
            if (_messageColor != value)
            {
                _messageColor = value;
                OnPropertyChanged(nameof(MessageColor));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateLogColor()
    {
        if (_messageColor == Colors.White || _messageColor == Colors.Black) // Default color based on theme
            MessageColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
    }
}