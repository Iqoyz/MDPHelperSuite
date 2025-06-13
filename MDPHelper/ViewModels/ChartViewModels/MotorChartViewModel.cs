using Microcharts;
using SkiaSharp;

namespace fyp_MDPHelperApp.ViewModels.ChartViewModels;

public class MotorChartViewModel : BaseChartViewModel
{
    private static List<ChartEntry> _rightMotorSpeedEntries = new();
    private static List<ChartEntry> _leftMotorSpeedEntries = new();

    public static MotorChartViewModel Instance { get; } = new();

    public List<ChartEntry> RightMotorSpeedEntries
    {
        get => _rightMotorSpeedEntries;
        private set
        {
            _rightMotorSpeedEntries = value;
            OnPropertyChanged(nameof(RightMotorSpeedEntries));
        }
    }

    public List<ChartEntry> LeftMotorSpeedEntries
    {
        get => _leftMotorSpeedEntries;
        private set
        {
            _leftMotorSpeedEntries = value;
            OnPropertyChanged(nameof(LeftMotorSpeedEntries));
        }
    }

    public void AddRightMotorSpeedEntry(DateTime dateTime, int rightWheelSpeed)
    {
        if (RightMotorSpeedEntries.Count > MAX_DATA_POINT) RightMotorSpeedEntries.RemoveRange(0, RightMotorSpeedEntries.Count - MAX_DATA_POINT);

        var timeLabel = dateTime.ToString("HH:mm:ss");

        RightMotorSpeedEntries.Add(new ChartEntry(rightWheelSpeed)
        {
            Label = timeLabel,
            ValueLabel = rightWheelSpeed.ToString(),
            ValueLabelColor = GetValueLabelColor(),
            Color = SKColor.Parse("#00BFFF")
        });
    }

    public void AddLeftMotorSpeedEntry(DateTime dateTime, int leftWheelSpeed)
    {
        if (LeftMotorSpeedEntries.Count > MAX_DATA_POINT) LeftMotorSpeedEntries.RemoveRange(0, LeftMotorSpeedEntries.Count - MAX_DATA_POINT);

        var timeLabel = dateTime.ToString("HH:mm:ss");

        LeftMotorSpeedEntries.Add(new ChartEntry(leftWheelSpeed)
        {
            Label = timeLabel,
            ValueLabel = leftWheelSpeed.ToString(),
            ValueLabelColor = GetValueLabelColor(),
            Color = SKColor.Parse("#FF1943")
        });
    }

    public override void UpdateEntriesValueLabelColor()
    {
        foreach (var entry in LeftMotorSpeedEntries) entry.ValueLabelColor = GetValueLabelColor();
        foreach (var entry in RightMotorSpeedEntries) entry.ValueLabelColor = GetValueLabelColor();

        OnPropertyChanged(nameof(LeftMotorSpeedEntries));
        OnPropertyChanged(nameof(RightMotorSpeedEntries));
    }

    public void ClearMotorData()
    {
        RightMotorSpeedEntries.Clear();
        LeftMotorSpeedEntries.Clear();
    }
}