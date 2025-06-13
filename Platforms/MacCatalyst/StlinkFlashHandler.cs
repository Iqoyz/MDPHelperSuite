using System.Diagnostics;

namespace fyp_MDPHelperApp.Services.PartialClasses;

public partial class StlinkFlashHandler
{
    private const string ResourcePath = "fyp_MDPHelperApp.Resources.FlashMethods.Stlink.stlinkMac.";

    private partial Task Run(string fileName, string arguments)
    {
        return RunAsync(fileName, arguments);
    }

    private async Task RunAsync(string fileName, string arguments)
    {
        var tempPath = Path.GetTempPath();
        var tempLibDir = Path.Combine(tempPath, "lib");
        var dylibPath = Path.Combine(tempLibDir, "libstlink.1.dylib");
        var dylibPath2 = Path.Combine(tempLibDir, "libstlink.1.7.0.dylib");
        var dylibPath3 = Path.Combine(tempLibDir, "libusb-1.0.0.dylib");
        var execFilePath = Path.Combine(tempPath, fileName);

        var sampleFileDir = Path.Combine(tempPath, "sample");
        var sampleFilePath = Path.Combine(sampleFileDir, "sample.bin");

        //Create folder if it doesn't exist
        // _logAction($"Creating directories at {tempLibDir}");
        Directory.CreateDirectory(tempLibDir);
        Directory.CreateDirectory(sampleFileDir);

        _logAction("Saving embedded library files.");
        await SaveEmbeddedFileAsync($"{ResourcePath}libstlink.1.dylib", dylibPath);
        await SaveEmbeddedFileAsync($"{ResourcePath}libstlink.1.7.0.dylib", dylibPath2);
        await SaveEmbeddedFileAsync($"{ResourcePath}libusb-1.0.0.dylib", dylibPath3);
        await SaveEmbeddedFileAsync($"{ResourcePath}{fileName}", execFilePath);
        await SaveEmbeddedFileAsync($"{ResourcePath}sample.bin", sampleFilePath);
        _logAction($"Running process {fileName} with arguments: {arguments}");
        await RunProcessAsync(tempPath, fileName, arguments);
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
            // _logAction($"Creating file stream for {destinationPath}.");
            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None,
                       4096, true))
            {
                await sourceStream.CopyToAsync(fileStream);
            }

            File.SetAttributes(destinationPath, FileAttributes.Normal);
            // _logAction($"File attributes set for {destinationPath}.");

            if (destinationPath.EndsWith("st-info") || destinationPath.EndsWith("st-flash"))
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

        await CleanUpTempFilesAsync();
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
            _currentProgress = 0.6; // Starting Flash write
            _updateProgressAction(_currentProgress);
        }
        else if (line.Contains("Starting verification of write complete"))
        {
            _currentProgress = 0.8; // Preparing for completion
            _updateProgressAction(_currentProgress);
        }
    }

    private async Task CleanUpTempFilesAsync()
    {
        // Define paths
        var tempPath = Path.GetTempPath();
        string[] filesAndDirs = { "st-info", "st-flash", "lib", "sample","mdpCustomFirmwareFile" };

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