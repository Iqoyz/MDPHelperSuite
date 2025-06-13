using System.Diagnostics;
using System.Text.RegularExpressions;

namespace fyp_MDPHelperApp.Services.PartialClasses;

public partial class UartFlashHandler
{
    private const string ResourcePath = "fyp_MDPHelperApp.Resources.FlashMethods.Uart.uartMac.";

    private partial Task Run(string fileName, string arguments)
    {
        return RunAsync(fileName, arguments);
    }

    private async Task RunAsync(string fileName, string arguments)
    {
        var tempPath = Path.GetTempPath();
        var execFilePath = Path.Combine(tempPath, fileName);

        var sampleFileDir = Path.Combine(tempPath, "sample");
        var sampleFilePath = Path.Combine(sampleFileDir, "sample.bin");

        //Create folder if it doesn't exist
        // _logAction($"Creating directories at {sampleFileDir}");
        Directory.CreateDirectory(sampleFileDir);

        _logAction("Saving embedded library files...");

        await SaveEmbeddedFileAsync($"{ResourcePath}{fileName}", execFilePath);
        await SaveEmbeddedFileAsync($"{ResourcePath}sample.bin", sampleFilePath);
        _logAction($"Running process {fileName} with arguments: {arguments}");

        try
        {
            await RunProcessAsync(tempPath, fileName, arguments);
        }
        finally
        {
            await CleanUpTempFilesAsync();
        }
    }

    private Stream? GetResourceStream(string resourceName)
    {
        var assembly = typeof(App).Assembly;
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) _logAction($"Resource stream not found: {resourceName}");

        return stream;
    }

    private async Task SaveEmbeddedFileAsync(string resourceName, string filePath)
    {
        // _logAction($"Retrieving resource stream for {resourceName}.");
        using (var stream = GetResourceStream(resourceName))
        {
            if (stream == null)
            {
                _logAction($"{resourceName} not found");
                throw new FileNotFoundException($"Resource {resourceName} not found.");
            }

            // _logAction($"Saving resource stream to {filePath}.");
            await SaveStreamToFileAsync(stream, filePath);

            if (!File.Exists(filePath))
            {
                _logAction($"Failed to save file to: {filePath}");
                throw new IOException($"Failed to save file to: {filePath}");
            }
        }
    }

    private async Task SaveStreamToFileAsync(Stream sourceStream, string destinationPath)
    {
        try
        {
            _logAction($"Creating file stream for {destinationPath}.");
            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None,
                       4096, true))
            {
                await sourceStream.CopyToAsync(fileStream);
            }

            File.SetAttributes(destinationPath, FileAttributes.Normal);
            _logAction($"File attributes set for {destinationPath}.");

            if (destinationPath.EndsWith("uartFlash"))
            {
                // _logAction($"Setting executable permissions for {destinationPath}.");
                var chmodProcessInfo = new ProcessStartInfo
                {
                    FileName = "/bin/chmod",
                    Arguments = $"+x \"{destinationPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var chmodProcess = new Process { StartInfo = chmodProcessInfo })
                {
                    chmodProcess.Start();
                    await chmodProcess.WaitForExitAsync();
                    _logAction($"Permissions set for {destinationPath}.");
                }
            }
        }
        catch (Exception ex)
        {
            _logAction($"Error saving file: {ex.Message}");
        }
    }

    private async Task RunProcessAsync(string filePath, string exeFileName, string arguments)
    {
        _logAction(
            $"Starting process with executable {Path.Combine(filePath, exeFileName)} and arguments: {arguments}");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(filePath, exeFileName),
            Arguments = arguments,
            WorkingDirectory = Path.GetDirectoryName(Path.Combine(filePath, exeFileName)),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        _currentProcess = new Process { StartInfo = processStartInfo };

        using (_currentProcess)
        {
            _currentProcess.Start();

            // Set up a cancellation token for the timeout (same as in the Windows version)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeToKillProccessSec)))
            {
                // Task for processing the output
                var outputTask = Task.Run(async () =>
                {
                    while (await _currentProcess.StandardOutput.ReadLineAsync() is { } line)
                    {
                        // Look for completion percentage in the output
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
                        }
                    }
                });

                // Handle process completion or timeout
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

            // Check if there were any errors
            var errorOutput = await _currentProcess.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(errorOutput)) _logAction($"Error: {errorOutput}");

            // Check if the process has completed successfully
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

    private async Task CleanUpTempFilesAsync()
    {
        // Define paths
        var tempPath = Path.GetTempPath();
        string[] filesAndDirs = { "uartFlash", "sample","mdpCustomFirmwareFile" };

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