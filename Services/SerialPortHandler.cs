using System.IO.Ports;

namespace fyp_MDPHelperApp.Services;

public class SerialPortHandler
{
    private readonly SerialPort _serialPort;

    public SerialPortHandler(string portName, int baudRate, Parity parity = Parity.None,
        int dataBits = 8, StopBits stopBits = StopBits.One)
    {
        _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
        {
            Handshake = Handshake.None,
            RtsEnable = false,
            DtrEnable = false,
            WriteTimeout = 5000 //5 seconds
        };
    }

    public void OpenPort()
    {
        if (!_serialPort.IsOpen) _serialPort.Open();
    }

    // Close the serial port
    public void ClosePort()
    {
        if (_serialPort.IsOpen) _serialPort.Close();
    }

    public string ReadData()
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        return _serialPort.ReadLine();
    }


    public void SetOnDataReceived(SerialDataReceivedEventHandler onDataReceived)
    {
        if (_serialPort == null)
            throw new InvalidOperationException("Serial port is not initialized.");

        _serialPort.DataReceived -= onDataReceived; // Remove any existing handler
        _serialPort.DataReceived += onDataReceived;
    }

    public async Task SendDataAsync(string data)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        await Task.Run(() =>
        {
            _serialPort.WriteLine(data); // Use _serialPort.Write if you don't want a newline character
        });
    }

    public static void EnterBootloaderMode(string portName, int ignoredBaudrate= 115200,Parity ignoredParity = Parity.None,
        int ignoredDatabit = 8, StopBits ignoredStopBits = StopBits.One)
    {
        using (var tempPort = new SerialPort(portName,ignoredBaudrate,ignoredParity,ignoredDatabit,ignoredStopBits))
        {
            tempPort.Handshake = Handshake.None;
            
            tempPort.Open();
            
            tempPort.RtsEnable = true;
            
            Thread.Sleep(100);
            
            tempPort.DtrEnable = true;
            
            tempPort.Close();
        }
    }

    public static string[] GetAvailPorts()
    {
        return SerialPort.GetPortNames();
    }

    public bool IsOpen()
    {
        return _serialPort.IsOpen;
    }

    public string GetPortName()
    {
        return _serialPort.PortName;
    }

    public int GetBaudrate()
    {
        return _serialPort.BaudRate;
    }
}