using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace fyp_MDPHelperApp.Services;

public class ToastMessageHandler
{
    public static async void ShowToastAsync(string text, ToastDuration duration = ToastDuration.Short,
        double fontSize = 14)
    {
        var cancellationTokenSource = new CancellationTokenSource();

        var toast = Toast.Make(text, duration, fontSize);

        await toast.Show(cancellationTokenSource.Token);
    }
}