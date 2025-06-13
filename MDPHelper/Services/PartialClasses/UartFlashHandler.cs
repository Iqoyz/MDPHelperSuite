using System.Diagnostics;

namespace fyp_MDPHelperApp.Services.PartialClasses;

public partial class UartFlashHandler
{
    private const double TimeToKillProccessSec = 201;

    private readonly Action<string> _logAction;

    private readonly Action<double> _updateProgressAction;

    private Process _currentProcess; // Store the running process

    private bool _isSuccessFlashed = true;

    public UartFlashHandler(Action<string> logAction, Action<double> updateProgressAction)
    {
        _logAction = logAction;
        _updateProgressAction = updateProgressAction;
    }

    public async Task ExecuteUartFlashAsync(string fileName, string arguments)
    {
        await Run(fileName, arguments);
    }

    public void CancelProcess()
    {
        if (_currentProcess != null && !_currentProcess.HasExited)
        {
            _logAction("User requested to kill the process...");
            _currentProcess.Kill();
            _logAction("Process has been killed.");
        }
        else
        {
            _logAction("No process is currently running.");
        }
    }

    public bool IsFlashSuccess()
    {
        return _isSuccessFlashed;
    }

    public void SetSuccessFlashed(bool successFlash)
    {
        _isSuccessFlashed = successFlash;
    }

    private partial Task Run(string fileName, string arguments);
}