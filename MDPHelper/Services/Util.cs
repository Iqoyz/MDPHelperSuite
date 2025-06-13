namespace fyp_MDPHelperApp.Services;

public class Util
{
    private static string TrimVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return version; // Return the original if null or empty
        }

        var parts = version.Split('.');

        return string.Join('.', parts.Take(parts.Length - 1));
    }

    public static string GetTrimVersion()
    {
        if (OperatingSystem.IsWindows())
            return TrimVersion(AppInfo.Current.VersionString);
        return AppInfo.Current.VersionString;
    }
}