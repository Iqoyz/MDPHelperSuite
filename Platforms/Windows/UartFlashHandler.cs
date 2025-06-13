using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace fyp_MDPHelperApp.Services.PartialClasses;

public partial class UartFlashHandler
{
    private const string ResourcePath = "fyp_MDPHelperApp.Resources.FlashMethods.Uart.uartWindows.";

    private partial Task Run(string fileName, string arguments)
    {
        return RunAsync(fileName, arguments);
    }

    private async Task RunAsync(string fileName, string arguments)
    {
        var resourceName = $"{ResourcePath}{fileName}";

        if (arguments.Contains("sample.bin"))
        {
            var sampleBinResourceName = $"{ResourcePath}sample.bin";
            using (var streamSample = await GetResourceStreamAsync(sampleBinResourceName))
            {
                if (streamSample != null)
                {
                    var sampleFileDir = Path.Combine(Path.GetTempPath(), "sample");
                    var sampleFilePath = Path.Combine(sampleFileDir, "sample.bin");

                    try
                    {
                        Directory.CreateDirectory(sampleFileDir);
                        await SaveStreamToFileAsync(streamSample, sampleFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logAction($"Error saving sample.bin: {ex.Message}");
                    }
                }
                else
                {
                    _logAction("Resource stream for sample.bin is null.");
                }
            }
        }


        // Extract the file from embedded resources
        using (var stream = await GetResourceStreamAsync(resourceName))
        {
            if (stream == null)
            {
                // Log or handle the error appropriately
                _logAction($"Executable file '{resourceName}' not found");
                return;
            }

            // Create a temporary file path
            var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            // Save the embedded file to a temporary location
            await SaveStreamToFileAsync(stream, tempFilePath);

            // Run the extracted file with the provided arguments
            try
            {
                await RunProcessAsync(tempFilePath, arguments);
            }
            catch (Exception ex)
            {
                _logAction($"Error running process: {ex.Message}");
            }
            finally
            {
                await CleanUpTempFilesAsync();
            }
        }
    }

    public async Task RunProcessAsync(string filePath, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = arguments,
            WorkingDirectory = Path.GetDirectoryName(filePath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        _currentProcess = new Process { StartInfo = processStartInfo };

        using (_currentProcess)
        {
            _currentProcess.Start();

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeToKillProccessSec)))
            {
                var outputTask = Task.Run(async () =>
                {
                    while (await _currentProcess.StandardOutput.ReadLineAsync() is { } line)
                    {
                        // Log output and progress (as before)
                        var percentageMatch = Regex.Match(line, @"(\d+\.\d+)%");
                        if (percentageMatch.Success)
                        {
                            var percentage = percentageMatch.Groups[1].Value;
                            if (double.TryParse(percentage, out var progress))
                            {
                                _updateProgressAction(progress / 100);
                                if (progress == 100.0) _logAction(line);
                            }
                        }
                        else
                        {
                            _logAction(line);
                            if (line.Contains("Erasing memory"))
                                _logAction("Flashing in progress...please wait patiently :)");
                        }
                    }
                });

                if (await Task.WhenAny(outputTask, Task.Delay(-1, cts.Token)) == outputTask)
                {
                    await _currentProcess.WaitForExitAsync();
                }
                else
                {
                    _logAction("Process timed out. Killing process...");
                    if (!_currentProcess.HasExited) _currentProcess.Kill();
                }
            }

            var errorOutput = await _currentProcess.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(errorOutput)) _logAction($"Error: {errorOutput}");

            if (_currentProcess.ExitCode == 0)
            {
                _logAction("Flash successfully completed.");
            }
            else
            {
                _isSuccessFlashed = false;
                _logAction("Failed to flash STM board. Please check your connection!");
            }
        }

        _currentProcess = null;
    }


    private async Task<Stream?> GetResourceStreamAsync(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(resourceName);
    }

    private async Task SaveStreamToFileAsync(Stream stream, string destinationPath)
    {
        using (var fileStream = File.Create(destinationPath))
        {
            await stream.CopyToAsync(fileStream);
        }
    }

    private async Task CleanUpTempFilesAsync()
    {
        // Define paths
        var tempPath = Path.GetTempPath();
        string[] filesAndDirs = { "uartFlash.exe", "sample", "mdpCustomFirmwareFile" };

        foreach (var item in filesAndDirs)
        {
            var fullPath = Path.Combine(tempPath, item);

            try
            {
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logAction($"Deleted file: {fullPath}");
                }
                else if (Directory.Exists(fullPath))
                {
                    // Recursively delete directory if it exists and is empty
                    await Task.Run(() => Directory.Delete(fullPath, true));
                    _logAction($"Deleted directory: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                _logAction($"Failed to delete {fullPath}: {ex.Message}");
            }
        }
    }
}