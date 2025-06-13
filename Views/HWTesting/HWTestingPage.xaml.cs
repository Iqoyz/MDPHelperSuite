using CommunityToolkit.Maui.Views;
using fyp_MDPHelperApp.Services;
using fyp_MDPHelperApp.Services.Api;
using fyp_MDPHelperApp.Services.PartialClasses;
using fyp_MDPHelperApp.ViewModels;
using fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

namespace fyp_MDPHelperApp.Views.HWTesting;

public enum FlashMethod
{
    Stlink_V2,
    UART,
    None
}

public enum TestingPart
{
    Motor,
    Servo,
    Ultrasonic_sensor,
    IRSensor,
    Motion_Sensors,
    Custom,
    None
}

public partial class HWTestingPage : ContentPage
{
    private readonly StlinkFlashHandler _exeStlinkFlashHandler;

    private readonly UartFlashHandler _exeUartFlashHandler;

    private readonly bool _isWindows = OperatingSystem.IsWindows();

    private string _command; //Command that sent by user to STM32 board through UART

    private CancellationTokenSource _countingLogTokenSource;

    private BaseTesting? _currentTestingPart;

    private FlashMethod _flashMethod = FlashMethod.None;

    private int _selectedBaudrate;

    private string _selectedPort;

    private SerialPortHandler _serialPortHandler;

    public HWTestingPage()
    {
        InitializeComponent();

        BindingContext = LogViewModel.Instance;

        Log(_isWindows ? "Running on Windows" : "Running on Mac");

        _exeStlinkFlashHandler = new StlinkFlashHandler(Log, UpdateProgressBar);
        _exeUartFlashHandler = new UartFlashHandler(Log, UpdateProgressBar);

        SizeChanged += OnPageSizeChanged;

        UpdateTpPickerSelection(TestingPart.None);
    }

    /*Log START*/
    private void Log(string message)
    {
        Console.WriteLine(message);

        string logMessage;
        if (message.Contains(DateTime.Now.ToString("HH:mm:ss")))
            logMessage = message;
        else
            logMessage = $"{DateTime.Now:HH:mm:ss} - {message}";

        if (BindingContext is not LogViewModel viewModel) return;

        var log = new Log { LogMessage = logMessage };

        // Override the default color dynamically
        if (message.Contains("Error", StringComparison.OrdinalIgnoreCase))
            log.MessageColor = Colors.Red;
        else if (message.Contains("Warning", StringComparison.OrdinalIgnoreCase)) log.MessageColor = Colors.Orange;

        Dispatcher.Dispatch(() => { viewModel.LogMessages.Add(log); });
    }

    private void LogStartCounting()
    {
        if (BindingContext is not LogViewModel viewModel) return;

        _countingLogTokenSource = new CancellationTokenSource();
        var token = _countingLogTokenSource.Token;

        var counter = 0; // Initialize counter

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                var tempCounter = counter;
                Dispatcher.Dispatch(() =>
                {
                    if (viewModel.LogMessages.Any() && viewModel.LogMessages.Last().LogMessage.StartsWith("Loading:"))
                    {
                        viewModel.LogMessages.RemoveAt(viewModel.LogMessages.Count - 1);
                        viewModel.LogMessages.Add(new Log { LogMessage = $"Loading: {tempCounter}" });
                    }
                    else
                    {
                        // Add a new log entry with the counting message if not present
                        viewModel.LogMessages.Add(new Log { LogMessage = $"Loading: {tempCounter}" });
                    }
                });

                counter++; // Increment the counter every 500ms
                await Task.Delay(500);

                if (ProgressBar.Progress > 0) break;
            }
        }, token);
    }

    private void LogStopCounting()
    {
        // Cancel the counting task if it's running
        _countingLogTokenSource?.Cancel();
    }

    private void LogScrollToEnd()
    {
        if (BindingContext is not LogViewModel viewModel) return;

        Dispatcher.Dispatch(() =>
        {
            LogCollectionView.ScrollTo(viewModel.LogMessages.Last(), position: ScrollToPosition.End, animate: true);
        });
    }
    /*Log END*/

    /*Button START*/
    private async void OnFlashMotorClicked(object sender, EventArgs e)
    {
        await StartFlash(TestingPart.Motor, "motor testing");
    }

    private async void OnMotorDataButtonClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new MotorDataChartPopUp());
    }

    private void OnClearMotorDataClicked(object sender, EventArgs e)
    {
        _currentTestingPart?.ClearData();
        Log("All motor speed data are cleared.");
    }

    private async void OnFlashServoClicked(object sender, EventArgs e)
    {
        await StartFlash(TestingPart.Servo, "servo testing");
    }

    private void OnClearLogButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is not LogViewModel viewModel) return;
        Dispatcher.Dispatch(() =>
        {
            viewModel.LogMessages.Clear(); // Clears all items from the collection  
        });
        Log(_isWindows ? "Running on Windows" : "Running on Mac");
    }

    private void OnScrollToEndButtonClicked(object sender, EventArgs e)
    {
        LogScrollToEnd();
    }

    private async void OnConnectButtonClicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync(new UARTConnectionSettingPopUp());

        if (result is Tuple<string, int> settings)
        {
            _selectedPort = settings.Item1;
            _selectedBaudrate = settings.Item2;
        }
        else
        {
            return;
        }

        if (_selectedPort != null && _selectedBaudrate == 0)
            _selectedBaudrate = 115200;

        Log("Connecting to " + _selectedPort + "...");
        await Task.Delay(800); //To make sure above log message is shown

        try
        {
            ConnectButton.IsEnabled = false;
            await ConnectPortAsync();
        }
        finally
        {
            ConnectButton.IsEnabled = true;
        }
    }

    private void OnDisconnectButtonClicked(object sender, EventArgs e)
    {
        if (_serialPortHandler == null || !_serialPortHandler.IsOpen())
        {
            Log("Not connecting to any port.");
            return;
        }

        try
        {
            if (_serialPortHandler != null && _serialPortHandler.IsOpen())
                Log($"Disconnected from {_serialPortHandler.GetPortName()}");

            DisconnectPort();

            _selectedPort = null;
        }
        catch (Exception ex)
        {
            Log($"Error: unable to disconnect {_selectedPort}: {ex.Message}");
        }
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        _exeUartFlashHandler.CancelProcess();
    }

    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        ShowMenu();
    }

    private async void SendCommandButtonClicked(object sender, EventArgs e)
    {
        if (_serialPortHandler == null)
        {
            Log("Please connect the port first :)");
            return;
        }

        if (string.IsNullOrEmpty(_command))
        {
            Log("Please enter command before sending.");
            return;
        }

        if (!IsValidCommand())
        {
            Log("Invalid command to send for " + TestingPartDropdown.SelectedItem + " testing.");
            return;
        }

        SendCommandButton.IsEnabled = false;
        Log("Sending " + _command + "...");
        try
        {
            await _serialPortHandler.SendDataAsync(_command);

            Log("Sent command: " + _command);
        }
        catch (Exception ignored)
        {
            Log("Unable to send command, please check your COM port selection.");
        }
        finally
        {
            SendCommandButton.IsEnabled = true;
        }
    }

    private async void OnFlashUltrasonicClicked(object sender, EventArgs e)
    {
        await StartFlash(TestingPart.Ultrasonic_sensor, "ultrasonic testing");
    }

    private async void OnUltrasonicDataAndResult(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new UltrasonicDataChartPopUp());
    }

    private void ClearUsDataClicked(object sender, EventArgs e)
    {
        _currentTestingPart?.ClearData();
        Log("All ultrasonic data are cleared");
    }

    private async void OnFlashIRTestingClicked(object? sender, EventArgs e)
    {
        await StartFlash(TestingPart.IRSensor, "ir testing");
    }

    private async void OnIRDataAndResult(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new IRDataChartPopUp());
    }

    private void ClearIRDataClicked(object sender, EventArgs e)
    {
        _currentTestingPart?.ClearData();
        Log("All IR data are cleared");
    }
    
    private async void OnFlashMotionTestingClicked(object? sender, EventArgs e)
    {
        await StartFlash(TestingPart.Motion_Sensors, "motion testing");
    }

    private async void OnMotionDataAndResult(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new MotionSensorDataChartPopup());
    }

    private void ClearMotionDataClicked(object sender, EventArgs e)
    {
        _currentTestingPart?.ClearData();
        Log("All motion sensor data are cleared");
    }

    private async void OnCustomFlashClicked(object sender, EventArgs e)
    {
        await StartFlash(TestingPart.Custom);
    }

    private async void OnCustomDataAndResult(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new CustomDataChartPopUp());
    }

    private void ClearCustomDataClicked(object sender, EventArgs e)
    {
        _currentTestingPart?.ClearData();
        Log("All custom data are cleared");
    }
    /*Button END*/


    /*Picker START*/
    private void FlashMethodDropdown_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var choice = FlashMethodDropdown.SelectedItem.ToString();

        switch (choice)
        {
            case "STLink/V2":
                _flashMethod = FlashMethod.Stlink_V2;
                break;
            case "COM Port":
                _flashMethod = FlashMethod.UART;
                if (_serialPortHandler == null || !_serialPortHandler.IsOpen())
                    Log("Please select COM port.");
                break;
        }
    }

    private void TestingPartDropdown_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateTpPickerSelection();
    }
    /*Picker END*/

    /*Other UI components START*/
    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        var windowHeight = Height;

        LogFrame.HeightRequest = windowHeight - 190; // Adjust the padding as needed
    }

    private void UpdateProgressBar(double progress)
    {
        Dispatcher.Dispatch(() =>
        {
            ProgressBar.ProgressTo(progress, 200, Easing.Linear); // Animate progress update, 200ms
        });
    }

    private void OnCommandInputEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        _command = CommandInputEditor.Text;
    }

    private void OnLogTraceCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            LogCollectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
            LogTraceLabel.Text = "Trace Log: ON";
            LogScrollToEnd();
        }
        else
        {
            LogCollectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
            LogTraceLabel.Text = "Trace Log: OFF";
        }
    }
    /*Other UI components END*/


    /*Helper function START*/
    //For flash operation START
    private async Task StartFlash(TestingPart part, string firmwareName = "")
    {
        LogStartCounting();

        if (!ValidateFlashMethod())
        {
            LogStopCounting();
            return;
        }

        if (_flashMethod == FlashMethod.UART && !ValidateComPort())
        {
            LogStopCounting();
            return;
        }

        string filePath;

        if (part == TestingPart.Custom)
            filePath = await PickFirmwareFileAsync();
        else
            filePath = await DownloadFirmwareFileAsync(firmwareName);

        if (filePath == null)
        {
            Log($"Error loading {firmwareName} firmware file.");
            LogStopCounting();
        }
        else
        {
            await HandleFlashOperationAsync(part, filePath);
        }
    }

    private bool ValidateFlashMethod()
    {
        if (_flashMethod == FlashMethod.None)
        {
            Log("Please select flashing method first.");
            LogStopCounting();
            return false;
        }

        return true;
    }

    private bool ValidateComPort()
    {
        if (_selectedPort == null)
        {
            Log("Please connect the port first :)");
            LogStopCounting();
            return false;
        }

        return true;
    }

    private async Task HandleFlashOperationAsync(TestingPart testingPart, string filePath)
    {
        UpdateProgressBar(0);
        ToggleAllFlashButtons();
        UpdateTpPickerSelection(TestingPart.None);

        try
        {
            SetUartFlashSuccess(true);
            SetStLinkFlashSuccess(true);
            if (_flashMethod == FlashMethod.Stlink_V2)
                await ExecuteStlinkFlashAsync(filePath);
            else if (_flashMethod == FlashMethod.UART) await ExecuteUartFlashAsync(filePath);
        }
        catch (Exception ex)
        {
            Log($"Unable to flash the board: {ex.Message}");
        }
        finally
        {
            LogStopCounting();
            ToggleAllFlashButtons();

            if (_flashMethod == FlashMethod.UART)
            {
                await ConnectPortAsync();
                CancelButton.IsEnabled = false;
            }

            if (IsFlashSuccess())
            {
                UpdateTpPickerSelection(testingPart);
                ShowMenu();
                ToastMessageHandler.ShowToastAsync("Finished flashing!");
            }
            else
            {
                ToastMessageHandler.ShowToastAsync("Failed to flash the board.");
            }
            
            //clean up downloaded firmwares
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ignored)
                {
                }
            }
        }
    }

    private async Task ExecuteStlinkFlashAsync(string filePath)
    {
        Log("Loading stlink flash...");
        Log("Please do not unplug the stlink when flashing...");

        var exeFileName = _isWindows ? "st-flash.exe" : "st-flash";
        var arguments = $"--reset write {filePath} 0x8000000";

        await _exeStlinkFlashHandler.ExecuteStlinkFlashAsync(exeFileName, arguments);
    }

    private async Task ExecuteUartFlashAsync(string filePath)
    {
        Log("Loading UART flash...");
        CancelButton.IsEnabled = true;

        DisconnectPort();

        var exeFileName = _isWindows ? "uartFlash.exe" : "uartFlash";
        var arguments = $"-R -i rts,dtr,:-dtr -w {filePath} -v -g 0x80000000 {_selectedPort}";

        if (!_isWindows)
        {
            SerialPortHandler.EnterBootloaderMode(_selectedPort);
            arguments = $"-R -w {filePath} -v -g 0x80000000 {_selectedPort}";
        }

        await _exeUartFlashHandler.ExecuteUartFlashAsync(exeFileName, arguments);
    }

    private bool IsFlashSuccess()
    {
        return _exeUartFlashHandler.IsFlashSuccess() && _exeStlinkFlashHandler.IsFlashSuccess();
    }

    private void SetUartFlashSuccess(bool flashSuccess)
    {
        _exeUartFlashHandler.SetSuccessFlashed(flashSuccess);
    }

    private void SetStLinkFlashSuccess(bool flashSuccess)
    {
        _exeStlinkFlashHandler.SetSuccessFlashed(flashSuccess);
    }
    //For flash operation END

    private void UpdateTpPickerSelection(TestingPart? testingPart = null)
    {
        if (testingPart.HasValue)
        {
            _currentTestingPart = testingPart switch
            {
                TestingPart.Motor => new MotorTesting(),
                TestingPart.Servo => new ServoTesting(),
                TestingPart.Ultrasonic_sensor => new UltrasonicTesting(),
                TestingPart.IRSensor => new IRTesting(),
                TestingPart.Motion_Sensors => new MotionTesting(),
                TestingPart.Custom => new CustomTesting(),
                _ => null
            };
        }
        else if (TestingPartDropdown.SelectedItem != null)
        {
            var choice = TestingPartDropdown.SelectedItem.ToString();
            _currentTestingPart = choice switch
            {
                "Motor" => new MotorTesting(),
                "Servo" => new ServoTesting(),
                "Ultrasonic Sensor" => new UltrasonicTesting(),
                "IR Sensor" => new IRTesting(),
                "Motion Sensors" => new MotionTesting(),
                "Custom" => new CustomTesting(),
                _ => null
            };
        }

        UpdateDropdownAndButton();
    }


    private void UpdateDropdownAndButton()
    {
        MenuButton.IsEnabled = false;
        MotorDataButton.IsEnabled = false;
        ClearMotorDataButton.IsEnabled = false;
        UltrasonicResultButton.IsEnabled = false;
        ClearUsDataButton.IsEnabled = false;
        IRResultButton.IsEnabled = false;
        ClearIRDataButton.IsEnabled = false;
        MotionResultButton.IsEnabled = false;
        ClearMotionDataButton.IsEnabled = false;
        CustomResultButton.IsEnabled = false;
        ClearCustomDataButton.IsEnabled = false;

        switch (_currentTestingPart)
        {
            case MotorTesting:
                TestingPartDropdown.SelectedItem = "Motor";
                CommandInputEditor.TextTransform = TextTransform.Uppercase;
                MenuButton.IsEnabled = true;
                MotorDataButton.IsEnabled = true;
                ClearMotorDataButton.IsEnabled = true;
                break;
            case ServoTesting:
                TestingPartDropdown.SelectedItem = "Servo";
                MenuButton.IsEnabled = true;
                break;
            case UltrasonicTesting:
                TestingPartDropdown.SelectedItem = "Ultrasonic Sensor";
                MenuButton.IsEnabled = true;
                CommandInputEditor.TextTransform = TextTransform.Uppercase;
                UltrasonicResultButton.IsEnabled = true;
                ClearUsDataButton.IsEnabled = true;
                break;
            case IRTesting:
                TestingPartDropdown.SelectedItem = "IR Sensor";
                MenuButton.IsEnabled = true;
                CommandInputEditor.TextTransform = TextTransform.Uppercase;
                IRResultButton.IsEnabled = true;
                ClearIRDataButton.IsEnabled = true;
                break;
            case MotionTesting:
                TestingPartDropdown.SelectedItem = "Motion Sensors";
                MenuButton.IsEnabled = true;
                CommandInputEditor.TextTransform = TextTransform.Uppercase;
                MotionResultButton.IsEnabled = true;
                ClearMotionDataButton.IsEnabled = true;
                break;
            case CustomTesting:
                TestingPartDropdown.SelectedItem = "Custom";
                MenuButton.IsEnabled = true;
                CustomResultButton.IsEnabled = true;
                ClearCustomDataButton.IsEnabled = true;
                break;
            default:
                TestingPartDropdown.SelectedItem = "None";
                CommandInputEditor.TextTransform = TextTransform.None;
                break;
        }
    }

    private void ShowMenu()
    {
        _currentTestingPart?.ShowMenu(Log);
    }

    private void ToggleAllFlashButtons()
    {
        FlashCustomButton.IsEnabled = !FlashCustomButton.IsEnabled;
        FlashServoTestingButton.IsEnabled = !FlashServoTestingButton.IsEnabled;
        FlashMotorTestingButton.IsEnabled = !FlashMotorTestingButton.IsEnabled;
        FlashUltrasonicButton.IsEnabled = !FlashUltrasonicButton.IsEnabled;
        FlashIRTestingButton.IsEnabled = !FlashIRTestingButton.IsEnabled;
        FlashMotionTestingButton.IsEnabled = !FlashMotionTestingButton.IsEnabled;
    }

    private async Task ConnectPortAsync()
    {
        if (string.IsNullOrEmpty(_selectedPort))
        {
            Log("No COM port selected");
            return;
        }

        if (_selectedBaudrate == 0)
        {
            Log("Please select your baudrate");
            return;
        }

        if (_serialPortHandler != null && _serialPortHandler.IsOpen() &&
            _selectedPort == _serialPortHandler.GetPortName() && _selectedBaudrate == _serialPortHandler.GetBaudrate())
        {
            Log($"COM port: {_selectedPort} is already connected");
            return;
        }

        SendCommandButton.IsEnabled = false;

        try
        {
            if (_serialPortHandler != null && _serialPortHandler.IsOpen())
                Log($"Disconnected from {_serialPortHandler.GetPortName()}");

            DisconnectPort();

            _serialPortHandler = new SerialPortHandler(_selectedPort, _selectedBaudrate);

            _serialPortHandler.SetOnDataReceived((sender, e) =>
            {
                var data = _serialPortHandler.ReadData();
                ProcessReceivedData(data);
            });

            await Task.Run(() => _serialPortHandler.OpenPort());
        }
        catch (Exception ex)
        {
            Log($"Error: unable to connect to {_selectedPort}: {ex.Message}");
            _selectedPort = null;
            SendCommandButton.IsEnabled = true;
        }
        finally
        {
            if (_serialPortHandler != null && _serialPortHandler.IsOpen()) Log(_selectedPort + " is connected.");
            SendCommandButton.IsEnabled = true;
        }
    }

    private void DisconnectPort()
    {
        // Clean up and close the serial port if open
        if (_serialPortHandler != null && _serialPortHandler.IsOpen())
        {
            _serialPortHandler.ClosePort();
            _serialPortHandler = null;
        }
    }

    private bool IsValidCommand()
    {
        if (_currentTestingPart == null) return true;

        if (!_currentTestingPart.IsCommandValid(_command)) return false;

        _command = _currentTestingPart.ProcessCommand(_command);

        return true;
    }


    private void ProcessReceivedData(string data)
    {
        _currentTestingPart?.ProcessData(data);

        Log(data);
    }

    private async Task<string> PickFirmwareFileAsync()
    {
        var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".bin", ".hex" } },
            {
                DevicePlatform.MacCatalyst, new[] { "bin", "hex" }
            }
        });

        var pickOptions = new PickOptions
        {
            PickerTitle = "Please select your firmware file (.bin or .hex)", //only mac os will show the picker title
            FileTypes = customFileType
        };

        try
        {
            var result = await FilePicker.Default.PickAsync(pickOptions);

            if (result == null) return null; // user cancel the file picker pop up

            var extension = Path.GetExtension(result.FileName).ToLowerInvariant();
            var targetFileName = extension == ".hex" ? "mdpCustomFirmwareFile.hex" : "mdpCustomFirmwareFile.bin";

            using (var stream = await result.OpenReadAsync())
            {
                var targetDir = Path.Combine(Path.GetTempPath(), "mdpCustomFirmwareFile");
                Directory.CreateDirectory(targetDir);
                var targetFilePath = Path.Combine(targetDir, targetFileName);

                using (var targetStream = File.Create(targetFilePath))
                {
                    await stream.CopyToAsync(targetStream);
                }

                return targetFilePath;
            }
        }
        catch (Exception ex)
        {
            Log($"Error picking file: {ex.Message}");
            return null;
        }
    }

    private async Task<string> DownloadFirmwareFileAsync(string firmwareName)
    {
        try
        {
            // Use the FirmwareApiClient to get the firmware file as a stream
            var stream = await FirmwareApiClient.GetFirmwareFileByNameAsync(firmwareName);

            if (stream == null)
            {
                Log($"Error downloading {firmwareName} firmware file.");
                return null;
            }

            var targetFileName = "mdpCustomFirmwareFile.bin";
            var targetDir = Path.Combine(Path.GetTempPath(), "mdpCustomFirmwareFile");

            Directory.CreateDirectory(targetDir);

            var targetFilePath = Path.Combine(targetDir, targetFileName);
            
            using (var targetStream = File.Create(targetFilePath))
            {
                await stream.CopyToAsync(targetStream);
            }

            return targetFilePath;
        }
        catch (Exception ex)
        {
            Log($"Error downloading firmware: {ex.Message}");
            return null;
        }
    }

    /*Helper function END*/
}