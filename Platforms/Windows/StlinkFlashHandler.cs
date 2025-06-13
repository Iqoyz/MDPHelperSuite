using System.Diagnostics;
using System.Reflection;

namespace fyp_MDPHelperApp.Services.PartialClasses;

public partial class StlinkFlashHandler
{
    private partial Task Run(string fileName, string arguments)
    {
        return RunAsync(fileName, arguments);
    }

    private async Task RunAsync(string fileName, string arguments)
    {
        var resourceName = $"fyp_MDPHelperApp.Resources.FlashMethods.Stlink.stlinkWindows.bin.{fileName}";

        if (arguments.Contains("sample.bin"))
        {
            var sampleBinResourceName = "fyp_MDPHelperApp.Resources.FlashMethods.Stlink.stlinkWindows.bin.sample.bin";
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

        using (var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true })
        {
            // Handlers for output and error data
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) ProcessLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null) ProcessLine(e.Data);
            };

            process.Start();

            // Begin asynchronous read
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logAction("Flash successfully completed.");
                _updateProgressAction(1.0); // Ensure progress is set to 100%
            }
            else
            {
                _isSuccessFlashed = false;
                _logAction("Failed to flash STM board. Please check your connection!");
            }
        }
    }

    private void ProcessLine(string line)
    {
        _logAction(line); // Log every line for debugging

        if (line.Contains("Attempting to write"))
        {
            _currentProgress = 0.1; // Start write
            _updateProgressAction(_currentProgress);
        }
        else if (line.Contains("Flash page at addr:"))
        {
            _currentProgress += 0.1;
            _updateProgressAction(Math.Min(_currentProgress, 0.8)); // Cap at 0.8
        }
        else if (line.Contains("Starting Flash write"))
        {
            _currentProgress = 0.6;
            _updateProgressAction(_currentProgress);
        }
        else if (line.Contains("Starting verification of write complete"))
        {
            _currentProgress = 0.8;
            _updateProgressAction(_currentProgress);
        }
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
        string[] filesAndDirs = { "st-info.exe", "st-flash.exe", "sample","mdpCustomFirmwareFile" };

        foreach (var item in filesAndDirs)
        {
            var fullPath = Path.Combine(tempPath, item);

            try
            {
                if (File.Exists(fullPath))
                    await Task.Run(() => File.Delete(fullPath));
                // _logAction($"Deleted file: {fullPath}");
                else if (Directory.Exists(fullPath))
                    // Recursively delete directory if it exists and is empty
                    await Task.Run(() => Directory.Delete(fullPath, true));
                // _logAction($"Deleted directory: {fullPath}"); 
            }
            catch (Exception ex)
            {
                _logAction($"Failed to delete {fullPath}: {ex.Message}");
            }
        }
    }
}