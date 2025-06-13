using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.Services;

namespace fyp_MDPHelperApp.Views.HWTesting;

public partial class UARTConnectionSettingPopUp : Popup
{
    private int _selectedBaudrate;

    private string _selectedPort;

    public UARTConnectionSettingPopUp()
    {
        InitializeComponent();

        UpdatePortsInComPortDropdown();

        UpdateBackgroundColor();
    }


    private void UpdateBackgroundColor()
    {
        if (Application.Current.RequestedTheme == AppTheme.Dark)
            Color = Color.FromHex("#212121"); // Gray900
        else
            Color = Color.FromHex("#FFFFFF"); // White
    }

    private void UpdatePortsInComPortDropdown()
    {
        var ports = SerialPortHandler.GetAvailPorts();

        var portsToRemove = new List<string>();

        foreach (var item in ComPortDropdown.Items)
            if (!ports.Contains(item))
                portsToRemove.Add(item);

        foreach (var port in portsToRemove) ComPortDropdown.Items.Remove(port);

        foreach (var port in ports)
            if (!ComPortDropdown.Items.Contains(port))
                ComPortDropdown.Items.Add(port);
    }

    private void ComPortDropdown_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _selectedPort = ComPortDropdown.SelectedItem.ToString();
    }

    private void BaudrateDropdown_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _selectedBaudrate = (int)BaudrateDropdown.SelectedItem;
    }

    private void OnRefreshButtonClicked(object sender, EventArgs e)
    {
        UpdatePortsInComPortDropdown();
    }

    private async void OnConnectButtonClicked(object sender, EventArgs e)
    {
        var result = new Tuple<string, int>(_selectedPort, _selectedBaudrate);
        await CloseAsync(result);
    }
}