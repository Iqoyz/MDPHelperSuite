using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.ViewModels.ChartViewModels;
using Microcharts;
using SkiaSharp;

namespace fyp_MDPHelperApp.Views.HWTesting;

public partial class MotionSensorDataChartPopup : Popup
{
    public MotionSensorDataChartPopup()
    {
        InitializeComponent();

        Color = DataChartPopUpUtil.UpdateInitBackgroundColor();

        MotionChartViewModel.Instance.UpdateEntriesValueLabelColor();

        if (MotionChartViewModel.Instance.XAxisEntries.Count == 0 &&
            MotionChartViewModel.Instance.YAxisEntries.Count == 0 &&
            MotionChartViewModel.Instance.ZAxisEntries.Count == 0)
        {
            EmptyMessageLabel.IsVisible = true;
            Size = DataChartPopUpUtil.EmptyPopupSize; // Smaller size when no data is available
            ChartTitleLabel.Text = "Motion Data";
        }
        else
        {
            double totalHeight = 20; // Initial margin or padding

            var titles = GetTitles();

// Handle case where only one title exists
            if (titles.Length == 1)
            {
                // Show only ChartView3
                DataScrollView1.IsVisible = false;
                XLabel.IsVisible = false;

                DataScrollView2.IsVisible = false;
                YLabel.IsVisible = false;

                DataScrollView3.IsVisible = true;
                ZLabel.IsVisible = true;
                ZLabel.Text = titles[0];

                var dataPointCount = MotionChartViewModel.Instance.ZAxisEntries.Count;
                ChartView3.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView3.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = MotionChartViewModel.Instance.ZAxisEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };

                totalHeight += ZLabel.Height + ChartView3.HeightRequest + 10; // Add spacing
            }
            else
            {
                // Handle case where multiple titles exist
                var chartEntries = new[]
                {
                    (DataScrollView: DataScrollView1, Label: XLabel, ChartView: ChartView1,
                        Entries: MotionChartViewModel.Instance.XAxisEntries,
                        Title: titles.Length > 0 ? titles[0] : null),
                    (DataScrollView: DataScrollView2, Label: YLabel, ChartView: ChartView2,
                        Entries: MotionChartViewModel.Instance.YAxisEntries,
                        Title: titles.Length > 1 ? titles[1] : null),
                    (DataScrollView: DataScrollView3, Label: ZLabel, ChartView: ChartView3,
                        Entries: MotionChartViewModel.Instance.ZAxisEntries,
                        Title: titles.Length > 2 ? titles[2] : null)
                };

                // Iterate over charts and configure visibility dynamically
                foreach (var (dataScrollView, label, chartView, entries, title) in chartEntries)
                    if (title != null) // Show chart only if there's a corresponding title
                    {
                        dataScrollView.IsVisible = true;
                        label.IsVisible = true;
                        label.Text = title;

                        var dataPointCount = entries.Count;
                        chartView.WidthRequest =
                            dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                        chartView.Chart = new LineChart
                        {
                            LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                            ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                            Entries = entries,
                            BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                        };

                        totalHeight += label.Height + chartView.HeightRequest + 10; // Add spacing
                    }
            }

            // Add final padding
            totalHeight += 80;
            Size = new Size(1200, totalHeight); // Set the popup size
        }
    }

    private string[] GetTitles()
    {
        string currentChartTitle = MotionChartViewModel.Instance.ChartTitle;
        switch (currentChartTitle)
        {
            case "gyro angle":
                ChartTitleLabel.Text = "Gyroscope angle data";
                return new[]
                {
                    "Roll Angle (degree)", "Pitch Angle (degree)", "Yaw Angle (degree)"
                };

            case "accel angle":
                ChartTitleLabel.Text = "Accelerometer angle data";
                return new[]
                {
                    "Roll Angle (degree)", "Pitch Angle (degree)", "Tilt Angle (degree)"
                };

            case "magnet angle":
                ChartTitleLabel.Text = "Magnetometer angle data";
                return new[]
                {
                    "Yaw Angle (degree)"
                };
            case "gyro raw":
                ChartTitleLabel.Text = "Gyroscope raw data";
                return new[]
                {
                    "X", "Y", "Z"
                };

            case "accel raw":
                ChartTitleLabel.Text = "Accelerometer raw data";
                return new[]
                {
                    "X", "Y", "Z"
                };

            case "magnet raw":
                ChartTitleLabel.Text = "Magnetometer raw data";
                return new[]
                {
                    "X", "Y", "Z"
                };
            default:
                return new string[] { };
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
        DataScrollView2.ScrollToAsync(e.ScrollX, 0, false);
        DataScrollView3.ScrollToAsync(e.ScrollX, 0, false);
        DataChartPopUpUtil.IsSyncingScroll = false;
    }

    private void OnScrollView2Scrolled(object sender, ScrolledEventArgs e)
    {
        if (DataChartPopUpUtil.IsSyncingScroll) return; // Prevent re-triggering
        DataChartPopUpUtil.IsSyncingScroll = true;
        DataScrollView1.ScrollToAsync(e.ScrollX, 0, false);
        DataScrollView3.ScrollToAsync(e.ScrollX, 0, false);
        DataChartPopUpUtil.IsSyncingScroll = false;
    }

    private void OnScrollView3Scrolled(object sender, ScrolledEventArgs e)
    {
        if (DataChartPopUpUtil.IsSyncingScroll) return; // Prevent re-triggering
        DataChartPopUpUtil.IsSyncingScroll = true;
        DataScrollView1.ScrollToAsync(e.ScrollX, 0, false);
        DataScrollView2.ScrollToAsync(e.ScrollX, 0, false);
        DataChartPopUpUtil.IsSyncingScroll = false;
    }

}