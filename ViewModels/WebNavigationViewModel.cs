using System.Windows.Input;

namespace fyp_MDPHelperApp.ViewModels;

public class WebNavigationViewModel
{
    public WebNavigationViewModel()
    {
        OpenWebCommand = new Command<string>(url =>
        {
            try
            {
                Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening URL: {ex.Message}");
            }
        });
    }

    public static WebNavigationViewModel Instance { get; } = new();

    public ICommand OpenWebCommand { get; }
}