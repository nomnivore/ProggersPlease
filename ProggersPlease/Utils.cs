using System;
using System.Diagnostics;

namespace ProggersPlease;

public static class Utils
{
    public static string SanitizeName(string name) {
        string sanitized = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9\s-]+", "");
        sanitized = sanitized.Replace(" ", "-").ToLower();
        return sanitized;
    }

    public static void OpenUrl(string url) {
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url, Verb = "open" });
    }

    public static string ToKey(string name, string world) {
        return $"{name}_{world}";
    }

    public static (string name, string world) FromKey(string key) {
        var parts = key.Split('_');
        if (parts.Length != 2) {
            throw new ArgumentException($"Invalid key format ({key}). Expected format: name_world");
        }
        return (parts[0], parts[1]);
    }
}
