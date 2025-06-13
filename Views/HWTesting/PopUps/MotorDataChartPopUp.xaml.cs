using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.ViewModels.ChartViewModels;
using Microcharts;

namespace fyp_MDPHelperApp.Views.HWTesting;

public partial class MotorDataChartPopUp : Popup
{
    public MotorDataChartPopUp()
    {
        InitializeComponent();

        Color = DataChartPopUpUtil.UpdateInitBackgroundColor();
        
        MotorChartViewModel.Instance.UpdateEntriesValueLabelColor();

        if (MotorChartViewModel.Instance.RightMotorSpeedEntries.Count == 0 &&
            MotorChartViewModel.Instance.LeftMotorSpeedEntries.Count == 0)
        {
            EmptyMessageLabel.IsVisible = true;
            Size = DataChartPopUpUtil.EmptyPopupSize; // Smaller size when no data is available
        }
        else
        {
            double totalHeight = 20; // Initial margin or padding

            if (MotorChartViewModel.Instance.RightMotorSpeedEntries.Count != 0)
            {
                DataScrollView1.IsVisible = true;
                RightSpeedLabel.IsVisible = true;
                var dataPointCount = MotorChartViewModel.Instance.RightMotorSpeedEntries.Count;
                ChartView1.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView1.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = MotorChartViewModel.Instance.RightMotorSpeedEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };
                totalHeight += RightSpeedLabel.Height + ChartView1.HeightRequest + 10; // Add spacing
            }

            if (MotorChartViewModel.Instance.LeftMotorSpeedEntries.Count != 0)
            {
                DataScrollView2.IsVisible = true;
                LeftSpeedLabel.IsVisible = true;
                var dataPointCount = MotorChartViewModel.Instance.LeftMotorSpeedEntries.Count;
                ChartView2.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView2.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = MotorChartViewModel.Instance.LeftMotorSpeedEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };
                totalHeight += LeftSpeedLabel.Height + ChartView2.HeightRequest + 10; // Add spacing
            }

            totalHeight += 80; // Additional padding
            Size = new Size(1200, totalHeight); // Full size when data is available
        }
    }

    private void UpdateBackgroundColor()
    {
        if (Application.Current.RequestedTheme == AppTheme.Dark)
            Color = Color.FromHex("#212121"); // Gray900
        else
            Color = Color.FromHex("#FFFFFF"); // White
    }

    private void OnScrollView1Scrolled(object sender, ScrolledEventArgs e)
    {
        if (DataChartPopUpUtil.IsSyncingScroll) return; // Prevent re-triggering
        DataChartPopUpUtil.IsSyncingScroll = true;
        DataScrollView2.ScrollToAsync(e.ScrollX, 0, false); // Sync ScrollView2 to ScrollView1
        DataChartPopUpUtil.IsSyncingScroll = false;
    }

    private void OnScrollView2Scrolled(object sender, ScrolledEventArgs e)
    {
        if (DataChartPopUpUtil.IsSyncingScroll) return; // Prevent re-triggering
        DataChartPopUpUtil.IsSyncingScroll = true;
        DataScrollView1.ScrollToAsync(e.ScrollX, 0, false); // Sync ScrollView1 to ScrollView2
        DataChartPopUpUtil.IsSyncingScroll = false;
    }
}