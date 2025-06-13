using fyp_MDPHelperApp.ViewModels.ChartViewModels;

namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public class CustomTesting : BaseTesting
{
    public override void ShowMenu(Action<string> logAction)
    {
        const string menu = "Please send your custom data in the following format:\n" +
                            "\"{title1}: %.2f, {title2}: %.2f, {title3}: %.2f, ...\\n\"\n" +
                            "You can include any number of data types, separated by commas.\n" +
                            " For example: \"gyro roll angle: 23.5, accel pitch angle: 55.3, Left_RPM: 300\\n\"\n" +
                            "Each data type will be visualized as a separate graph.\n\n";
        logAction(menu);
    }

    public override void ProcessData(string data)
    {
        //Data format: "title1: %.2f, title2: %.2f, title3: %.2f\n"
        //not limit to just three titles

        var parts = data.Split(',');
        var parsedData = new Dictionary<string, double>();

        foreach (var part in parts)
        {
            var titleValue = part.Trim().Split(':');
            if (titleValue.Length == 2)
            {
                var title = titleValue[0].Trim(); // Extract the title

                // Try to parse the value as a double (can handle both float and integer)
                if (double.TryParse(titleValue[1].Trim(), out var value)) parsedData[title] = value;
            }
        }

        // Pass the parsed data to your graphing logic
        foreach (var (title, value) in parsedData)
            // Replace "AddGraphEntry" with your graphing method
            CustomChartViewModel.Instance.AddGraphEntry(title, value);
    }

    public override void ClearData()
    {
        CustomChartViewModel.Instance.ClearCustomData();
    }
}