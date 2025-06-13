using Microcharts;
using SkiaSharp;

namespace fyp_MDPHelperApp.ViewModels.ChartViewModels;

public class UltrasonicChartViewModel : BaseChartViewModel
{
    private static List<ChartEntry> _usDistanceEntries = new();
    private static List<ChartEntry> _usResponseTimeEntries = new();
    public static UltrasonicChartViewModel Instance { get; } = new();

    public List<ChartEntry> UsDistanceEntries
    {
        get => _usDistanceEntries;
        private set
        {
            _usDistanceEntries = value;
            OnPropertyChanged(nameof(UsDistanceEntries));
        }
    }

    public List<ChartEntry> UsResponseTimeEntries
    {
        get => _usResponseTimeEntries;
        private set
        {
            _usResponseTimeEntries = value;
            OnPropertyChanged(nameof(UsResponseTimeEntries));
        }
    }

    public void AddUsDistanceEntry(int distance)
    {
        if (UsDistanceEntries.Count > MAX_DATA_POINT) UsDistanceEntries.RemoveRange(0, UsDistanceEntries.Count - MAX_DATA_POINT);

        var timeLabel = DateTime.Now.ToString("HH:mm:ss");

        UsDistanceEntries.Add(new ChartEntry(distance)
        {
            Label = timeLabel,
            ValueLabel = distance.ToString(),
            ValueLabelColor = GetValueLabelColor(),
            Color = SKColor.Parse("#00BFFF")
        });
    }

    public void AddUsResponseTimeEntry(float responseTime)
    {
        if (UsResponseTimeEntries.Count > MAX_DATA_POINT) UsResponseTimeEntries.RemoveRange(0, UsResponseTimeEntries.Count - MAX_DATA_POINT);

        var timeLabel = DateTime.Now.ToString("HH:mm:ss");

        UsResponseTimeEntries.Add(new ChartEntry(responseTime)
        {
            Label = timeLabel,
            ValueLabel = responseTime.ToString(),
            ValueLabelColor = GetValueLabelColor(),
            Color = SKColor.Parse("#FF1943")
        });
    }

    public override void UpdateEntriesValueLabelColor()
    {
        foreach (var entry in UsDistanceEntries) entry.ValueLabelColor = GetValueLabelColor();
        foreach (var entry in UsResponseTimeEntries) entry.ValueLabelColor = GetValueLabelColor();

        OnPropertyChanged(nameof(UsDistanceEntries));
        OnPropertyChanged(nameof(UsResponseTimeEntries));
    }
}