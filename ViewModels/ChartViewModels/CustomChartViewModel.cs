using Microcharts;
using SkiaSharp;

namespace fyp_MDPHelperApp.ViewModels.ChartViewModels;

public class CustomChartViewModel : BaseChartViewModel
{
    // Dictionary to hold chart entries for each title
    private static Dictionary<string, List<ChartEntry>> _chartEntries = new();

    public static CustomChartViewModel Instance { get; } = new();

    public Dictionary<string, List<ChartEntry>> ChartEntries
    {
        get => _chartEntries;
        private set
        {
            _chartEntries = value;
            OnPropertyChanged(nameof(ChartEntries));
        }
    }

    public void AddGraphEntry(string title, double value)
    {
        if (!_chartEntries.ContainsKey(title))
            // Initialize the chart entries list for this title
            _chartEntries[title] = new List<ChartEntry>();

        // Limit the number of entries for this chart
        if (_chartEntries[title].Count > MAX_DATA_POINT) _chartEntries[title].RemoveRange(0, _chartEntries[title].Count - MAX_DATA_POINT);

        var timeLabel = DateTime.Now.ToString("HH:mm:ss");

        // Add a new entry to the chart
        _chartEntries[title].Add(new ChartEntry((float)value)
        {
            Label = timeLabel,
            ValueLabel = value.ToString("F2"),
            ValueLabelColor = GetValueLabelColor(),
            Color = GetDynamicColorForTitle(title)
        });

        OnPropertyChanged(nameof(ChartEntries));
    }

    public void ClearCustomData()
    {
        _chartEntries.Clear(); // Clears both keys and values from the dictionary
        OnPropertyChanged(nameof(ChartEntries));
    }

    public override void UpdateEntriesValueLabelColor()
    {
        foreach (var entryList in _chartEntries.Values)
        foreach (var entry in entryList)
            entry.ValueLabelColor = GetValueLabelColor();

        OnPropertyChanged(nameof(ChartEntries));
    }

    private SKColor GetDynamicColorForTitle(string title)
    {
        // Generate a color based on the hash code of the title
        var hash = title.GetHashCode();
        var r = (byte)((hash & 0xFF0000) >> 16);
        var g = (byte)((hash & 0x00FF00) >> 8);
        var b = (byte)(hash & 0x0000FF);
        return new SKColor(r, g, b);
    }
}