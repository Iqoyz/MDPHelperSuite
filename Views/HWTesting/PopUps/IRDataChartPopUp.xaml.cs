using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.ViewModels.ChartViewModels;
using Microcharts;

namespace fyp_MDPHelperApp.Views.HWTesting;

public partial class IRDataChartPopUp : Popup
{
    public IRDataChartPopUp()
    {
        InitializeComponent();

        Color = DataChartPopUpUtil.UpdateInitBackgroundColor();
        
        if (IRChartViewModel.Instance.IRCalibrationEntries.Count == 0 &&
            IRChartViewModel.Instance.IRDistanceEntries.Count == 0)
        {
            EmptyMessageLabel.IsVisible = true;
            Size = DataChartPopUpUtil.EmptyPopupSize; // Smaller size when no data is available
        }
        else
        {
            double totalHeight = 20; // Initial margin or padding

            if (IRChartViewModel.Instance.IRCalibrationEntries.Count != 0)
            {
                IRCalibrationLabel.IsVisible = true;
                DataScrollView1.IsVisible = true;
                var dataPointCount = IRChartViewModel.Instance.IRCalibrationEntries.Count;
                ChartView1.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView1.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = IRChartViewModel.Instance.IRCalibrationEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };

                totalHeight += IRCalibrationLabel.Height + ChartView1.HeightRequest + 10; // Add spacing
            }

            if (IRChartViewModel.Instance.IRDistanceEntries.Count != 0)
            {
                DataScrollView2.IsVisible = true;
                DistanceDataLabel.IsVisible = true;
                var dataPointCount = IRChartViewModel.Instance.IRDistanceEntries.Count;
                ChartView2.WidthRequest = dataPointCount * DataChartPopUpUtil.DataPointSpacing; // Set dynamic width
                ChartView2.Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = IRChartViewModel.Instance.IRDistanceEntries,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                };
                totalHeight += DistanceDataLabel.Height + ChartView2.HeightRequest + 10; // Add spacing
            }

            totalHeight += 80; // Additional padding
            Size = new Size(1200, totalHeight); // Full size when data is available
        }
    }
}