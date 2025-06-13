namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public class ServoTesting : BaseTesting
{
    public override void ShowMenu(Action<string> logAction)
    {
        const string menu = "\nServo Testing Command:\n" +
                            "Enter a positive integer PWM  value (e.g. 130) to set the servo position.";

        const string warning = "⚠️ Warning: Entering values outside the recommended range of 80 to 250 " +
                               "may damage the servo. Please proceed with caution.\n\n";
        logAction(menu);
        logAction(warning);
    }

    public override bool IsCommandValid(string command)
    {
        return int.TryParse(command, out var angle) && angle >= 0;
    }

    public override string ProcessCommand(string command)
    {
        return $"SERVO {command}";
    }
}