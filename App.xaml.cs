using System.Diagnostics;
using fyp_MDPHelperApp.Services;
using Application = Microsoft.Maui.Controls.Application;

namespace fyp_MDPHelperApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        CheckForUpdates();
    }

    private async void CheckForUpdates()
    {
        var (isUpdateAvailable, downloadUrl) = await UpdateHandler.CheckForUpdatesAsync();
        if (isUpdateAvailable)
        {
            ToastMessageHandler.ShowToastAsync("Update available. Downloading...");
            var mainModuleFileName = Process.GetCurrentProcess().MainModule?.FileName;

            if (mainModuleFileName != null)
            {
                var currentAppPath = mainModuleFileName;
                var fileNameWithExtension = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);

                var newFileDir = Path.Combine(Path.GetTempPath(), $"{fileNameWithExtension}");

                await UpdateHandler.PerformUpdateAsync(downloadUrl, newFileDir, currentAppPath);
            }
        }
        else
        {
            ToastMessageHandler.ShowToastAsync("Your app is up to date.");
        }
    }
}