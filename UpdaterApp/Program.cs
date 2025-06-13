using System.Diagnostics;

class Program
{
    static async Task Main(string[] args)
    {
        if (!ValidateArguments(args, out string currentExePath, out string newExePath, out bool isWindows))
        {
            Console.WriteLine("Usage: UpdaterApp.exe <currentExePath> <newExePath> <isWindows>");
            return;
        }
        Console.WriteLine($"current exe file path: {currentExePath}");
        Console.WriteLine($"new exe file path: {newExePath}");
        Console.WriteLine("Waiting for the main app to close...");
        
        //wait for currect app to exit
        await WaitForProcessToExit(Path.GetFileNameWithoutExtension(currentExePath));

        try
        {
            if (isWindows)
            {
                ReplaceApp(newExePath, currentExePath);
                // await WaitForProcessToExit(Path.GetFileNameWithoutExtension(currentExePath));
            }
            else
            {
                LaunchInstaller(newExePath);
            }
            Console.WriteLine("Update complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during the update: {ex.Message}");
        }
        finally
        {
            if(isWindows)
                CleanupTempFile(newExePath);

            if (Environment.ProcessPath != null) DeleteSelf(Environment.ProcessPath);
        }
    }

    private static bool ValidateArguments(string[] args, out string currentExePath, out string newExePath, out bool isWindows)
    {
        currentExePath = string.Empty;
        newExePath = string.Empty;
        isWindows = false;

        if (args.Length >= 3 && bool.TryParse(args[2], out isWindows))
        {
            currentExePath = args[0];
            newExePath = args[1];
            return true;
        }

        return false;
    }

    private static async Task WaitForProcessToExit(string processName)
    {
        while (IsProcessRunning(processName))
        {
            Console.WriteLine($"Waiting for process '{processName}' to exit...");
            await Task.Delay(1000); // Check every second
        }
    }

    private static void ReplaceApp(string newExePath, string currentExePath)
    {
        Console.WriteLine("Replacing the old version...");
        Console.WriteLine($"newExePath: {newExePath}");
        Console.WriteLine($"currentExePath: {currentExePath}");
        File.Move(newExePath, currentExePath, true);
        
        Console.WriteLine("Launching the updated app...");
        Process.Start(new ProcessStartInfo
        {
            FileName = currentExePath,
            UseShellExecute = true
        });
    }

    private static void LaunchInstaller(string newPkgPath)
    {
        Console.WriteLine("Launching the installer...");

        try
        {
            // Ensure the .pkg file has correct permissions
            var chmodProcess = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{newPkgPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var chmod = Process.Start(chmodProcess);
            chmod?.WaitForExit();

            // Use the 'open' command to launch the .pkg installer
            var openProcess = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{newPkgPath}\"",
                UseShellExecute = true
            };

            using var process = Process.Start(openProcess);
            process?.WaitForExit();

            Console.WriteLine("Installer launched. Follow the on-screen instructions to complete the update.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while launching the installer: {ex.Message}");
        }
    }

    private static bool IsProcessRunning(string processName)
    {
        return Process.GetProcessesByName(processName).Any();
    }

    private static void CleanupTempFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Console.WriteLine("Temporary file deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete temporary file: {ex.Message}");
            }
        }
    }
    
    private static void DeleteSelf(string updaterAppPath)
    {
        if (OperatingSystem.IsWindows())
        {
            // Create a batch file for self-deletion
            var tempScriptPath = Path.Combine(Path.GetTempPath(), "delete_updater.bat");

            File.WriteAllText(tempScriptPath, $@"
        @echo off
        timeout /t 2 > nul
        del ""{updaterAppPath}"" > nul 2>&1
        del ""{tempScriptPath}"" > nul 2>&1
        ");

            // Launch the batch file
            var startInfo = new ProcessStartInfo
            {
                FileName = tempScriptPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
            
            Console.WriteLine("Scheduled self-deletion (Windows).");
        }
        else
        {
            // Create a shell script for self-deletion
            var tempScriptPath = Path.Combine(Path.GetTempPath(), "delete_updater.sh");

            File.WriteAllText(tempScriptPath, $@"
        #!/bin/bash
        sleep 2
        rm ""{updaterAppPath}""
        rm -- ""{tempScriptPath}""
        ");

            // Make the script executable
            var chmodProcess = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{tempScriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process.Start(chmodProcess)?.WaitForExit();

            // Launch the shell script
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{tempScriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);

            Console.WriteLine("Scheduled self-deletion (macOS/Linux).");
        }
    }

}
