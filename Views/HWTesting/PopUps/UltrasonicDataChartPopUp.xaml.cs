using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.ViewModels.ChartViewModels;
using Microcharts;

namespace fyp_MDPHelperApp.Views.HWTesting;

public partial class UltrasonicDataChartPopUp : Popup
{
    public UltrasonicDataChartPopUp()
    {
        InitializeComponent();

        Color = DataChartPopUpUtil.UpdateInitBackgroundColor();

        UltrasonicChartViewModel.Instance.UpdateEntriesValueLabelColor();

        if (UltrasonicChartViewModel.Instance.UsDistanceEntries.Count == 0 &&
            UltrasonicChartViewModel.Instance.UsResponseTimeEntries.Count == 0)
        {
            EmptyMessageLabel.IsVisible = true;
            Size = new Size(1200, 200); // Smaller size when no data is available
        }
        else
        {
            double totalHeight = 20; // Initial margin or padding
            if (UltrasonicChartViewModel.Instance.UsDistanceEntries.Count != 0)
            {
                DataScrollView1.IsVisible = true;
                DistanceDataLabel.IsVisible = true;
                var dataPointCount = UltrasonicChartViewModel.Instance.UsDistanceEntries.Count;
                ChartView1.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView1.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = UltrasonicChartViewModel.Instance.UsDistanceEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };
                totalHeight += DistanceDataLabel.Height + ChartView1.HeightRequest + 10; // Add spacing
            }

            if (UltrasonicChartViewModel.Instance.UsResponseTimeEntries.Count != 0)
            {
                DataScrollView2.IsVisible = true;
                ResponseTimeLabel.IsVisible = true;
                var dataPointCount = UltrasonicChartViewModel.Instance.UsResponseTimeEntries.Count;
                ChartView2.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView2.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = UltrasonicChartViewModel.Instance.UsResponseTimeEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };
                totalHeight += ResponseTimeLabel.Height + ChartView2.HeightRequest + 10; // Add spacing
            }

            totalHeight += 80; //additional padding
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