using fyp_MDPHelperApp.ViewModels.ChartViewModels;

namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public class UltrasonicTesting : BaseTesting
{
    public override void ShowMenu(Action<string> logAction)
    {
        const string menu = "\nUltrasonic Sensor Testing command:\n" +
                            "\t1.MANUAL\n" +
                            "Please enter your command (e.g. MANUAL) and press send button:\n\n";
        logAction(menu);
    }

    public override bool IsCommandValid(string command)
    {
        var parts = command.Split(' ');

        if (parts.Length == 1 && parts[0].ToUpper() == "MANUAL") return true;

        return false;
    }

    public override void ProcessData(string data)
    {
        var chartViewModel = UltrasonicChartViewModel.Instance;

        // Split the data by commas
        var parts = data.Split(',');

        // Ensure the message has exactly two parts: "Distance: %d cm" and "Response Time: %d us"
        if (parts.Length == 2)
        {
            // Parse the first part for distance
            var distancePart = parts[0].Trim().Split(' ');
            if (distancePart.Length == 3 &&
                distancePart[0].ToUpper() == "DISTANCE:" &&
                distancePart[2].ToUpper() == "CM" &&
                int.TryParse(distancePart[1], out var distance) &&
                distance != 0)
            {
                chartViewModel.AddUsDistanceEntry(distance);

                // Parse the second part for response time
                var responseTimePart = parts[1].Trim().Split(' ');
                if (responseTimePart.Length == 4 &&
                    responseTimePart[0].ToUpper() == "RESPONSE" &&
                    responseTimePart[1].ToUpper() == "TIME:" &&
                    responseTimePart[3].ToUpper() == "US" &&
                    int.TryParse(responseTimePart[2], out var responseTime) &&
                    responseTime != 0)
                    chartViewModel.AddUsResponseTimeEntry(responseTime);
            }
        }
    }

    public override void ClearData()
    {
        UltrasonicChartViewModel.Instance.UsDistanceEntries.Clear();
        UltrasonicChartViewModel.Instance.UsResponseTimeEntries.Clear();
    }
}