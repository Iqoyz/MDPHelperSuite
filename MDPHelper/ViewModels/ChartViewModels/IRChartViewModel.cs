using Microcharts;
using SkiaSharp;

namespace fyp_MDPHelperApp.ViewModels.ChartViewModels;

public class IRChartViewModel : BaseChartViewModel
{
    private static List<ChartEntry> _irCalibrationEntries = new();
    private static List<ChartEntry> _irDistanceEntries = new();
    
    public static IRChartViewModel Instance { get; } = new();
    
    public List<ChartEntry> IRCalibrationEntries
    {
        get => _irCalibrationEntries;
        private set
        {
            _irCalibrationEntries = value;
            OnPropertyChanged(nameof(IRCalibrationEntries));
        }
    }

    public List<ChartEntry> IRDistanceEntries
    {
        get => _irDistanceEntries;
        private set
        {
            _irDistanceEntries = value;
            OnPropertyChanged(nameof(IRDistanceEntries));
        }
    }
    
    public void AddIRDistanceEntry(int distance)
    {
        if (IRDistanceEntries.Count > MAX_DATA_POINT) IRDistanceEntries.RemoveRange(0, IRDistanceEntries.Count - MAX_DATA_POINT);
        
        var timeLabel = DateTime.Now.ToString("HH:mm:ss");

        IRDistanceEntries.Add(new ChartEntry(distance)
        {
            Label = timeLabel,
            ValueLabel = distance.ToString(),
            ValueLabelColor = GetValueLabelColor(),
            Color = SKColor.Parse("#00BFFF")
        });
    }
    
    public void AddCalibrationEntry(int knownDistance, int adcAvg)
    {
        if (IRCalibrationEntries.Count > MAX_DATA_POINT) IRCalibrationEntries.RemoveRange(0, IRCalibrationEntries.Count - MAX_DATA_POINT);
        
        IRCalibrationEntries.Add(new ChartEntry(adcAvg)
        {
            Label = knownDistance + "cm", // Convert int to string
            ValueLabel = adcAvg.ToString("F2"), // Format float for better readability
            ValueLabelColor = GetValueLabelColor(),
            Color = SKColor.Parse("#FF1943")
        });
    }
    
    public void ClearIRSensorData()
    {
        IRCalibrationEntries.Clear();
        IRDistanceEntries.Clear();
        OnPropertyChanged(nameof(IRCalibrationEntries));
        OnPropertyChanged(nameof(IRDistanceEntries));
    }

    public void ClearIRCalibrationData()
    {
        IRCalibrationEntries.Clear();
        OnPropertyChanged(nameof(IRCalibrationEntries));
    }


    public override void UpdateEntriesValueLabelColor()
    {
        foreach (var entry in IRCalibrationEntries) entry.ValueLabelColor = GetValueLabelColor();
        foreach (var entry in IRDistanceEntries) entry.ValueLabelColor = GetValueLabelColor();

        OnPropertyChanged(nameof(IRCalibrationEntries));
        OnPropertyChanged(nameof(IRDistanceEntries));
    }
}