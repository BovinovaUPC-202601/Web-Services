using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Configuration;

namespace VacApp.Tests.IntegrationTests;

internal static class LocalModelTestSettings
{
    public static LocalModelSettings Load()
    {
        var settings = new LocalModelSettings();
        var envValues = ReadEnvFile();

        settings.BaseUrl = GetSetting("LM_STUDIO_BASE_URL", envValues) ?? settings.BaseUrl;
        settings.Model = GetSetting("LM_STUDIO_MODEL", envValues) ?? settings.Model;
        settings.ApiKey = GetSetting("LM_STUDIO_API_KEY", envValues) ?? settings.ApiKey;

        return settings;
    }

    private static string? GetSetting(string key, IReadOnlyDictionary<string, string> envValues)
    {
        return Environment.GetEnvironmentVariable(key) ??
               (envValues.TryGetValue(key, out var value) ? value : null);
    }

    private static IReadOnlyDictionary<string, string> ReadEnvFile()
    {
        var envPath = FindEnvPath();
        if (envPath is null) return new Dictionary<string, string>();

        return File.ReadLines(envPath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }

    private static string? FindEnvPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var envPath = Path.Combine(directory.FullName, "VacApp-Bovinova-Platform", ".env");
            if (File.Exists(envPath)) return envPath;

            directory = directory.Parent;
        }

        return null;
    }
}
