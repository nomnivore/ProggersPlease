using System.Diagnostics;

namespace ProggersPlease;

public static class Utils
{
    public static string SanitizeName(string name) {
        string sanitized = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9\s]", "");
        sanitized = sanitized.Replace(" ", "-").ToLower();
        return sanitized;
    }

    public static void OpenUrl(string url) {
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url, Verb = "open" });
    }
}
