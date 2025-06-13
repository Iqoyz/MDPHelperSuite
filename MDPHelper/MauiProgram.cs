using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

namespace fyp_MDPHelperApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if WINDOWS
        string tempWebView2Folder = Path.Combine(Path.GetTempPath(), "WebView2Cache");
        Directory.CreateDirectory(tempWebView2Folder);
        // Set WebView2 user data folder to the temp path
        Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", tempWebView2Folder);
#endif

        
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(options => { options.SetShouldEnableSnackbarOnWindows(true); })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMicrocharts();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}