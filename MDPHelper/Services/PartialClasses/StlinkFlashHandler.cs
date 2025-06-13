namespace fyp_MDPHelperApp.Services.PartialClasses;

public partial class StlinkFlashHandler
{
    private readonly Action<string> _logAction;

    private readonly Action<double> _updateProgressAction;

    private double _currentProgress;

    private bool _isSuccessFlashed = true;

    public StlinkFlashHandler(Action<string> logAction, Action<double> updateProgressAction)
    {
        _logAction = logAction;
        _updateProgressAction = updateProgressAction;
    }

    public async Task ExecuteStlinkFlashAsync(string fileName, string arguments)
    {
        await Run(fileName, arguments);
    }

    public bool IsFlashSuccess()
    {
        return _isSuccessFlashed;
    }

    public void SetSuccessFlashed(bool successFlash)
    {
        _isSuccessFlashed = successFlash;
    }

    // Partial method declarations
    private partial Task Run(string fileName, string arguments);
}