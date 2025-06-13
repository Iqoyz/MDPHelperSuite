using fyp_MDPHelperApp.Services;
using fyp_MDPHelperApp.ViewModels.ChartViewModels;

namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public class IRTesting : BaseTesting
{
    public override void ShowMenu(Action<string> logAction)
    {
        const string menu = "\nIR Sensor Testing command:\n" +
                            "\t1. MANUAL\n" +
                            "\t2. CALIBRATE {interval in cm}\n" +
                            "Please enter your command (e.g., MANUAL, CALIBRATE 10) and press the send button:\n\n";
        logAction(menu);
        ToastMessageHandler.ShowToastAsync($"count = {IRChartViewModel.Instance.IRDistanceEntries.Count}");
    }

    public override bool IsCommandValid(string command)
    {
        var parts = command.Split(' ');

        if (parts.Length == 1 && parts[0].ToUpper() == "MANUAL") 
            return true;

        if (parts.Length == 2 && parts[0].ToUpper() == "CALIBRATE")
        {
            return int.TryParse(parts[1], out int interval) && interval > 0;
        }

        return false;
    }


    public override void ClearData()
    {
        IRChartViewModel.Instance.ClearIRSensorData();
    }

    public override string ProcessCommand(string command)
    {
        if(command == "CALIBRATE") IRChartViewModel.Instance.ClearIRCalibrationData();
        return command;
    }

    public override void ProcessData(string data)
    {
        data = data.Trim();  // Remove whitespace and newlines
        
        // Process IR Distance
        if (data.StartsWith("IR Distance:"))
        {
            var parts = data.Trim().Split(' ');  // Trim ensures no extra whitespace

            if (parts.Length == 4 &&                // Expecting "IR Distance: 35 cm"
                parts[0] == "IR" &&
                parts[1] == "Distance:" &&
                int.TryParse(parts[2], out var distance) &&  // Parse clean integer
                parts[3] == "cm" && distance > 0)            // Ensure correct suffix
            {
                IRChartViewModel.Instance.AddIRDistanceEntry(distance);
            }
        }
        
        // Process Known Distance and ADC
        else if (data.StartsWith("Known Distance:"))
        {
            var parts = data.Split('|');  // Split by '|'

            if (parts.Length == 2)
            {
                // Process "Known Distance" part
                var distPart = parts[0].Trim().Split(' ');  // "Known Distance: 20 cm"
                var adcPart = parts[1].Trim().Split(' ');   // "ADC: 974"

                if (distPart.Length == 4 &&                 // Ensure correct split
                    distPart[0] == "Known" &&
                    distPart[1] == "Distance:" &&
                    int.TryParse(distPart[2], out var knownDistance) &&  // Extract distance
                    distPart[3] == "cm" && knownDistance > 0 &&          // Ensure "cm" suffix
                    adcPart.Length == 2 &&
                    adcPart[0] == "ADC:" &&
                    int.TryParse(adcPart[1], out var adcAvg))            // Extract ADC value
                {
                    IRChartViewModel.Instance.AddCalibrationEntry(knownDistance, adcAvg);
                }
            }
        }
    }

}