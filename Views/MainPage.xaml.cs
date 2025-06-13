using fyp_MDPHelperApp.Services;

namespace fyp_MDPHelperApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        versionLabel.Text = "Welcome to MDP Helper " + Util.GetTrimVersion();;
    }
}