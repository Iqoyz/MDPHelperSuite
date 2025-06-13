using fyp_MDPHelperApp.Services;
using fyp_MDPHelperApp.ViewModels;
using fyp_MDPHelperApp.Views.About;

namespace fyp_MDPHelperApp;

public partial class AppShell : Shell
{
    private readonly ThemeViewModel _themeViewModel;

    public AppShell()
    {
        InitializeComponent();
        SetupNavigationView();

        Routing.RegisterRoute("UserGuidePage", typeof(HWTestingGuidePage));
        Routing.RegisterRoute("QnAPage", typeof(QnAPage));

        _themeViewModel = ThemeViewModel.Instance;
        BindingContext = _themeViewModel;
        
        versionLabel.Text  = Util.GetTrimVersion();
        
        //run whenever theme changed
        Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;

#if WINDOWS //hide the on/off text of the switch
  Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping("NoLabel", (handler, View) =>
  {
      handler.PlatformView.OnContent = null;
      handler.PlatformView.OffContent = null;

      // remove the padding around the switch as well
      handler.PlatformView.MinWidth = 0;
  });
#endif
    }

    private void SetupNavigationView()
    {
#if WINDOWS
        Loaded += delegate
		{
			var navigationView = (Microsoft.UI.Xaml.Controls.NavigationView)flyout.Handler!.PlatformView!;
			navigationView.IsPaneToggleButtonVisible = true;
			navigationView.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Auto;
		};
#endif
    }


    private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
    {
        _themeViewModel.IsDarkTheme = e.Value;
    }

    private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        // Update log theme colors globally
        foreach (var log in LogViewModel.Instance.LogMessages) log.UpdateLogColor();
    }
}