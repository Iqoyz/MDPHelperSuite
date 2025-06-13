using fyp_MDPHelperApp.ViewModels.ChartViewModels;

namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public class MotionTesting : BaseTesting
{
    private static string _previousType = string.Empty; // Store the previous title

    private static readonly HashSet<string> ValidCommands = new()
    {
        "ACCEL ANG", "GYRO ANG", "MAG ANG",
        "ACCEL RAW", "GYRO RAW", "MAG RAW",
        "GYRO DRIFT"
    };

    public override void ShowMenu(Action<string> logAction)
    {
        const string menu = "\nMotion Sensor Testing Commands:\n" +
                            "\t1. ACCEL ANG  - Accelerometer angles (Roll, Pitch, Tilt in degrees)\n" +
                            "\t2. GYRO ANG   - Gyroscope angles (Roll, Pitch, Yaw in degrees)\n" +
                            "\t3. MAG ANG    - Magnetometer angle (not accurate, Yaw in degrees)\n" +
                            "\t4. ACCEL RAW  - Raw accelerometer data \n" +
                            "\t5. GYRO RAW   - Raw gyroscope data \n" +
                            "\t6. MAG RAW    - Raw magnetometer data \n" +
                            "\t7. GYRO DRIFT - Gyroscope drift calculation (degrees per second)\n" +
                            "Enter command (e.g. ACCEL RAW, GYRO DRIFT) and press send:";

        const string warning =
            "⚠️ Warning: Switching between different commands (except 'GYRO DRIFT') will reset the motion data.\n\n";
        logAction(menu);
        logAction(warning);
    }


    public override bool IsCommandValid(string command)
    {
        return ValidCommands.Contains(command.ToUpper());
    }

    public override void ProcessData(string data)
    {
        // Example formats:
        // "gyro raw: 0.12, 0.45, 0.78"
        // "accel raw: 0.01, 0.02, 0.03"
        // "magnet raw: -0.01, 0.00, 0.01"
        // "gyro: 0.12,0.45,0.78"

        // Split data into type and values
        var parts = data.Split(':', 2); // Split into type and value part
        if (parts.Length != 2) return;

        var type = parts[0].Trim().ToLower(); // Extract and normalize the sensor type
        var valuesPart = parts[1].Trim();

        // Split values and ensure there are exactly 3 components
        var values = valuesPart.Split(',').Select(v => v.Trim()).ToArray();
        if (values.Length != 3) return;

        // Parse X, Y, Z values
        if (!float.TryParse(values[0], out var x) ||
            !float.TryParse(values[1], out var y) ||
            !float.TryParse(values[2], out var z))
            return; // Return if parsing fails

        // Normalize type for consistency
        if (!type.EndsWith("raw") &&
            (type == "gyro" || type == "accel" ||
             type == "magnet")) type += " angle"; // Add 'angle' to standard format types for clarity

        if (_previousType != type)
        {
            _previousType = type;
            ClearData();
        }

        // Update the chart with parsed data
        MotionChartViewModel.Instance.AddMotionSensorEntry(DateTime.Now, x, y, z, type);
    }


    public override void ClearData()
    {
        MotionChartViewModel.Instance.ClearMotionSensorData();
    }
}