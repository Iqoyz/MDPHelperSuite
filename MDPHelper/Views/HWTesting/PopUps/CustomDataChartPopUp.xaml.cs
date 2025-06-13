using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.ViewModels.ChartViewModels;
using Microcharts;
using Microcharts.Maui;

namespace fyp_MDPHelperApp.Views.HWTesting;

public partial class CustomDataChartPopUp : Popup
{
    private readonly List<ScrollView> _scrollViews = new(); // Store references to all ScrollViews

    public CustomDataChartPopUp()
    {
        InitializeComponent();

        UpdateBackgroundColor();

        CustomChartViewModel.Instance.UpdateEntriesValueLabelColor();

        if (CustomChartViewModel.Instance.ChartEntries.Count == 0)
        {
            EmptyMessageLabel.IsVisible = true;
            Size = DataChartPopUpUtil.EmptyPopupSize; // Smaller size when no data is available
            ChartsContainer.IsVisible = false;
        }
        else
        {
            RenderCharts();
            EmptyMessageLabel.IsVisible = false;
        }
    }

    private void UpdateBackgroundColor()
    {
        if (Application.Current.RequestedTheme == AppTheme.Dark)
            Color = Color.FromHex("#212121"); // Gray900
        else
            Color = Color.FromHex("#FFFFFF"); // White
    }

    private void RenderCharts()
    {
        double totalHeight = 20; // Initial padding or margin
        var chartIndex = 0;

        foreach (var chartEntry in CustomChartViewModel.Instance.ChartEntries)
        {
            // Create a label for the chart title
            var chartLabel = new Label
            {
                Text = chartEntry.Key,
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ChartsContainer.Children.Add(chartLabel);

            // Create a ScrollView for the chart
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                HeightRequest = 350
            };

            scrollView.Scrolled += OnScrollViewScrolled;

            // Create a ChartView for the entries
            var chartView = new ChartView
            {
                WidthRequest = chartEntry.Value.Count * DataChartPopUpUtil.DataPointSpacing, // Set dynamic width
                Chart = new LineChart
                {
                    LabelTextSize = DataChartPopUpUtil.ChartLabelSize,
                    ValueLabelTextSize = DataChartPopUpUtil.ChartValueLabelSize,
                    Entries = chartEntry.Value,
                    BackgroundColor = DataChartPopUpUtil.GetBackgroundColorForChart()
                }
            };

            // Add the ChartView to the ScrollView
            scrollView.Content = chartView;

            // Add the ScrollView to the container
            ChartsContainer.Children.Add(scrollView);

            // Store reference for syncing
            _scrollViews.Add(scrollView);

            // Adjust total height
            totalHeight += chartLabel.Height + scrollView.HeightRequest + 10; // Add spacing

            chartIndex++;
        }

        totalHeight += 80; // Additional padding
        Size = new Size(1200, totalHeight); // Adjust popup size dynamically
    }

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (DataChartPopUpUtil.IsSyncingScroll) return; // Prevent recursive scrolling
        DataChartPopUpUtil.IsSyncingScroll = true;

        // Sync all other ScrollViews to the current scroll position
        var currentScrollView = sender as ScrollView;
        foreach (var scrollView in _scrollViews)
            if (scrollView != currentScrollView)
                scrollView.ScrollToAsync(e.ScrollX, 0, false);

        DataChartPopUpUtil.IsSyncingScroll = false;
    }
}