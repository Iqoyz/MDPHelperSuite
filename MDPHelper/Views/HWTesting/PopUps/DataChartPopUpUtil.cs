using SkiaSharp;

namespace fyp_MDPHelperApp.Views.HWTesting;

public class DataChartPopUpUtil
{
    public static readonly int DataPointSpacing = 50; // Pixels per data point
    public static readonly int ChartLabelSize = 20;
    public static readonly int ChartValueLabelSize = 20;
    public static bool IsSyncingScroll; // Prevent infinite loop while syncing scroll

    public static readonly Size EmptyPopupSize = new(1200, 200);

    public static SKColor GetBackgroundColorForChart()
    {
        return Application.Current.RequestedTheme == AppTheme.Dark
            ? SKColor.Parse("#212121")
            : SKColors.White;
    }
    
    public static Color UpdateInitBackgroundColor()
    {
        if (Application.Current.RequestedTheme == AppTheme.Dark)
            return Color.FromHex("#212121"); // Gray900
        return Color.FromHex("#FFFFFF"); // White
    }
}