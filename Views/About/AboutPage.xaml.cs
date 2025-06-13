using fyp_MDPHelperApp.Services;
using fyp_MDPHelperApp.ViewModels;

namespace fyp_MDPHelperApp.Views.About;

public partial class AboutPage : ContentPage
{
    public const string FeedbackFormUrl = "https://forms.office.com/r/JqDYiafDVt";

    public AboutPage()
    {
        InitializeComponent();

        QrCodeImage.Source = QrCodeHandler.GenerateQrCode(FeedbackFormUrl);

        BindingContext = WebNavigationViewModel.Instance;

        versionLabel.Text = "About MDP Helper (Beta)" + Util.GetTrimVersion();;
    }

    private async void OnNavigateToUserGuideTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("UserGuidePage");
    }

    private async void OnNavigateToQnATapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("QnAPage");
    }
}