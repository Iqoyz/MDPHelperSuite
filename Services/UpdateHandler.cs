using System.Diagnostics;
using fyp_MDPHelperApp.Services.Api;
using CommunityToolkit.Maui.Core;

namespace fyp_MDPHelperApp.Services;

public class UpdateHandler
{
    /// <summary>
    /// Checks for updates by comparing the current version with the latest version.
    /// </summary>
    public static async Task<(bool IsUpdateAvailable, string DownloadUrl)> CheckForUpdatesAsync()
    {
        try
        {
            var isWindows = OperatingSystem.IsWindows();
            string fileType = isWindows ? ".exe" : ".pkg";
            string? fileName = isWindows ? "MDPHelper" : null;

            string query = $"fileType={fileType}";
            if (!string.IsNullOrEmpty(fileName))
            {
                query += $"&fileName={fileName}";
            }

            var latestAppInfo = await AppinfoApiClient.GetLatestAppInfoAsync(query);

            // Compare versions
            var currentVersion = new Version(Util.GetTrimVersion());
            var latestVersion = new Version(latestAppInfo.Version);

            bool isUpdateAvailable = latestVersion > currentVersion;

            return (isUpdateAvailable, latestAppInfo.DownloadUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while checking for updates: {ex.Message}");
            return (false, string.Empty);
        }
    }

    /// <summary>
    /// Performs the update process by downloading the new executable and launching the updater app.
    /// </summary>
    public static async Task PerformUpdateAsync(string newExeUrl, string newExePath, string currentExePath)
    {
        await NotifyAndDownloadUpdateAsync(newExeUrl, newExePath);

        try
        {
            var updaterAppPath = await GetUpdaterAppPathAsync();
            LaunchUpdaterApp(updaterAppPath, currentExePath, newExePath);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to launch the updater app: {ex.Message}";
            Console.WriteLine(errorMessage);
            ToastMessageHandler.ShowToastAsync(errorMessage);
        }
        finally
        {
            var message = OperatingSystem.IsWindows()
                ? "The update has been downloaded. Please close the app to allow the update process to complete."
                : "The update has been downloaded. Please close the app to install the updated version.";
            Console.WriteLine("Now close the app to install the update");
            ToastMessageHandler.ShowToastAsync(message, ToastDuration.Long);
        }
    }

    private static async Task NotifyAndDownloadUpdateAsync(string newExeUrl, string newExePath)
    {
        try
        {
            await DownloadFileAsync(newExeUrl, newExePath);
        }
        finally
        {
            Console.WriteLine("Downloaded updates successfully.");
        }
    }

    private static async Task<string> GetUpdaterAppPathAsync()
    {
        Console.WriteLine("Determining updater app path...");

        var isWindows = OperatingSystem.IsWindows();
        string fileType = isWindows ? ".exe" : ".app";
        string fileName = "fyp-UpdaterApp";

        // Construct query parameters
        string query = $"fileType={fileType}";
        query += $"&fileName={fileName}";

        var latestAppInfo = await AppinfoApiClient.GetLatestAppInfoAsync(query);

        if (latestAppInfo == null || string.IsNullOrEmpty(latestAppInfo.DownloadUrl))
        {
            Console.WriteLine("Error: Could not retrieve updater app information from the API.");
            throw new Exception("Updater app information not found.");
        }

        var downloadsFolder = Path.Combine(Path.GetTempPath(), "Updater");
        if (!Directory.Exists(downloadsFolder))
        {
            Directory.CreateDirectory(downloadsFolder);
        }

        var filenameWithExtension = isWindows ? fileName + fileType : fileName;

        var updaterFilePath = Path.Combine(downloadsFolder, filenameWithExtension);

        Console.WriteLine($"Downloading updater app to: {updaterFilePath}");

        await DownloadFileAsync(latestAppInfo.DownloadUrl, updaterFilePath);
        Console.WriteLine($"Updater app downloaded successfully: {updaterFilePath}");

        return updaterFilePath;
    }

    private static void LaunchUpdaterApp(string updaterAppPath, string currentExePath, string newExePath)
    {
        // Ensure executable permission on macOS
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                Console.WriteLine("Setting executable permissions for macOS...");
                var chmodProcess = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{updaterAppPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(chmodProcess);
                process?.WaitForExit();

                if (process?.ExitCode != 0)
                {
                    Console.WriteLine($"Failed to set executable permissions. Error: {process?.StandardError.ReadToEnd()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting executable permissions: {ex.Message}");
                throw;
            }
        }

        var isWindowsArgument = OperatingSystem.IsWindows() ? "true" : "false";

        var startInfo = new ProcessStartInfo
        {
            FileName = updaterAppPath,
            Arguments = $"\"{currentExePath}\" \"{newExePath}\" {isWindowsArgument}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine($"Launching updater app with paths:");
        Console.WriteLine($"- Current EXE: {currentExePath}");
        Console.WriteLine($"- New EXE: {newExePath}");

        using var processToLaunch = Process.Start(startInfo);
        if (processToLaunch != null)
        {
            Console.WriteLine($"Updater app launched successfully with Process ID: {processToLaunch.Id}");
        }
        else
        {
            Console.WriteLine("Failed to launch the updater app.");
        }
    }

    private static async Task DownloadFileAsync(string fileUrl, string filePath)
    {
        await ApiClientUtility.DownloadFileUsingUrlAsync(fileUrl, filePath);
    }
}
