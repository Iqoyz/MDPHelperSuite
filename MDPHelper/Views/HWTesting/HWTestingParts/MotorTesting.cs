using fyp_MDPHelperApp.ViewModels.ChartViewModels;

namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public class MotorTesting : BaseTesting
{
    public override void ShowMenu(Action<string> logAction)
    {
        const string menu = "\nMotor Testing command:\n" +
                            "\t1. FW {time (seconds) to move forward}\n" +
                            "\t2. BW {time (seconds) to move backward}\n" +
                            "\t3. FW INDEF\n" +
                            "\t4. BW INDEF\n" +
                            "\t5. FL {time (seconds) to left wheel move forward}\n" + // Left wheel forward
                            "\t6. FR {time (seconds) to right wheel move forward}\n" + // Right wheel forward
                            "\t7. BL {time (seconds) to left wheel move backward}\n" + // Left wheel backward
                            "\t8. BR {time (seconds) to right wheel move backward}\n" + // Right wheel backward
                            "\t9. FL INDEF\n" + // Left wheel forward indefinitely
                            "\t10. FR INDEF\n" + // Right wheel forward indefinitely
                            "\t11. BL INDEF\n" + // Left wheel backward indefinitely
                            "\t12. BR INDEF\n" + // Right wheel backward indefinitely
                            "\t13. SPEED {CUSTOM_SPEED} (Set custom speed for both wheels, default speed: 3000, Max custom speed allowed: 5500)\n" +
                            "\t14. SPEED LEFT {CUSTOM_SPEED} (Set custom speed for left wheel only)\n" +
                            "\t15. SPEED RIGHT {CUSTOM_SPEED} (Set custom speed for right wheel only)\n" +
                            "\t16. CHECK SPEED (Check current speed for both wheels)\n" +
                            "Please enter your command (e.g. FW 10, BL INDEF, SPEED 1700, SPEED LEFT 1200, CHECK SPEED) and press send button:\n\n";
        logAction(menu);
    }


    public override bool IsCommandValid(string command)
    {
        // Handle SPEED commands: SPEED {CUSTOM_SPEED}, SPEED LEFT {CUSTOM_SPEED}, SPEED RIGHT {CUSTOM_SPEED}
        if (command.StartsWith("CHECK"))
        {
            var split = command.Split(' ');
            if (split.Length == 2 && split[1] == "SPEED") return true;
            return false;
        }

        if (command.StartsWith("SPEED"))
        {
            var split = command.Split(' ');
            if (split.Length == 2 || split.Length == 3)
            {
                if (split.Length == 2 &&
                    int.TryParse(split[1], out var speedValue) &&
                    speedValue >= 0 && speedValue <= 5500)
                    return true;

                if (split.Length == 3 &&
                    (split[1] == "LEFT" || split[1] == "RIGHT") &&
                    int.TryParse(split[2], out speedValue) &&
                    speedValue >= 0 && speedValue <= 5500)
                    return true;
            }

            return false;
        }

        // Handle movement commands: FW, BW, FL, FR, BL, BR with time or INDEF
        var validCommands = new HashSet<string> { "FW", "BW", "FL", "FR", "BL", "BR" };
        var parts = command.Split(' ');

        if (parts.Length == 2 && validCommands.Contains(parts[0]))
        {
            // Check for valid duration (numeric value)
            if (decimal.TryParse(parts[1], out _)) return true;

            // Check for indefinite command
            if (parts[1] == "INDEF") return true;
        }

        return false;
    }


    public override void ProcessData(string speedData)
    {
        if (!speedData.StartsWith("Left_RPM:")) return; // Only process if the data starts with "Left_RPM:"

        var parts = speedData.Split(',');
        if (parts.Length == 2) // Ensure there are exactly two parts
        {
            var leftWheelPart = parts[0].Trim().Split(':');
            var rightWheelPart = parts[1].Trim().Split(':');

            // Check and parse left wheel speed
            if (leftWheelPart.Length == 2 && leftWheelPart[0] == "Left_RPM" &&
                int.TryParse(leftWheelPart[1].Trim(), out var leftWheelSpeed))
                // Check and parse right wheel speed
                if (rightWheelPart.Length == 2 && rightWheelPart[0] == "Right_RPM" &&
                    int.TryParse(rightWheelPart[1].Trim(), out var rightWheelSpeed))
                {
                    // Add to chart only if speed is outside the -10 to 10 range
                    if (Math.Abs(leftWheelSpeed) >= 3)
                        MotorChartViewModel.Instance.AddLeftMotorSpeedEntry(DateTime.Now, leftWheelSpeed);

                    if (Math.Abs(rightWheelSpeed) >= 3)
                        MotorChartViewModel.Instance.AddRightMotorSpeedEntry(DateTime.Now, rightWheelSpeed);
                }
        }
    }

    public override void ClearData()
    {
        MotorChartViewModel.Instance.ClearMotorData();
    }
}