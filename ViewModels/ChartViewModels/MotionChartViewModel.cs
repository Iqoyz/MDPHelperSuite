using Microcharts;
using SkiaSharp;

namespace fyp_MDPHelperApp.ViewModels.ChartViewModels;

public class MotionChartViewModel : BaseChartViewModel
{
    private static List<ChartEntry> _xAxisEntries = new();
    private static List<ChartEntry> _yAxisEntries = new();
    private static List<ChartEntry> _zAxisEntries = new();
    public static MotionChartViewModel Instance { get; } = new();

    public List<ChartEntry> XAxisEntries => _xAxisEntries;
    public List<ChartEntry> YAxisEntries => _yAxisEntries;
    public List<ChartEntry> ZAxisEntries => _zAxisEntries;

    private static string _chartTitle = "";

    public string ChartTitle
    {
        get => _chartTitle;
        private set
        {
            _chartTitle = value;
            OnPropertyChanged(nameof(ChartTitle));
        }
    }

    public void AddMotionSensorEntry(DateTime dateTime, float x, float y, float z, string chartTitle)
    {
        var timeLabel = dateTime.ToString("HH:mm:ss");

        // Update chart title only if it changes
        if (_chartTitle != chartTitle)
        {
            ChartTitle = chartTitle;
        }

        _xAxisEntries.Add(new ChartEntry(x)
        {
            Label = timeLabel,
            ValueLabel = x.ToString("F2"),
            Color = SKColor.Parse("#00BFFF"), // Blue for X-axis
            ValueLabelColor = GetValueLabelColor()
        });

        _yAxisEntries.Add(new ChartEntry(y)
        {
            Label = timeLabel,
            ValueLabel = y.ToString("F2"),
            Color = SKColor.Parse("#FF1943"), // Red for Y-axis
            ValueLabelColor = GetValueLabelColor()
        });

        _zAxisEntries.Add(new ChartEntry(z)
        {
            Label = timeLabel,
            ValueLabel = z.ToString("F2"),
            Color = SKColor.Parse("#00FF00"), // Green for Z-axis
            ValueLabelColor = GetValueLabelColor()
        });

        // Limit entries to the latest 300 data points
        if (_xAxisEntries.Count > MAX_DATA_POINT) _xAxisEntries.RemoveRange(0, _xAxisEntries.Count - MAX_DATA_POINT);
        if (_yAxisEntries.Count > MAX_DATA_POINT) _yAxisEntries.RemoveRange(0, _yAxisEntries.Count - MAX_DATA_POINT);
        if (_zAxisEntries.Count > MAX_DATA_POINT) _zAxisEntries.RemoveRange(0, _zAxisEntries.Count - MAX_DATA_POINT);

        OnPropertyChanged(nameof(XAxisEntries));
        OnPropertyChanged(nameof(YAxisEntries));
        OnPropertyChanged(nameof(ZAxisEntries));
    }

    public void ClearMotionSensorData()
    {
        _xAxisEntries.Clear();
        _yAxisEntries.Clear();
        _zAxisEntries.Clear();
        ChartTitle = "";  // Clear chart title
        OnPropertyChanged(nameof(XAxisEntries));
        OnPropertyChanged(nameof(YAxisEntries));
        OnPropertyChanged(nameof(ZAxisEntries));
        OnPropertyChanged(nameof(ChartTitle));
    }

    public override void UpdateEntriesValueLabelColor()
    {
        foreach (var entry in _xAxisEntries) entry.ValueLabelColor = GetValueLabelColor();
        foreach (var entry in _yAxisEntries) entry.ValueLabelColor = GetValueLabelColor();
        foreach (var entry in _zAxisEntries) entry.ValueLabelColor = GetValueLabelColor();

        OnPropertyChanged(nameof(XAxisEntries));
        OnPropertyChanged(nameof(YAxisEntries));
        OnPropertyChanged(nameof(ZAxisEntries));
    }
}
