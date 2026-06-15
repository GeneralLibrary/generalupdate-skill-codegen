using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// Strategy 1: Standard Client-Server update.
/// Suitable for most scenarios with a backend server.
///
/// Backend required:
/// - POST /Upgrade/Verification — version verification
/// - (Optional) POST /Upgrade/Report — status reporting
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class ClientServerStrategy
{
    public static async Task RunAsync()
    {
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource("https://your-server.com/api", "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)
            .SetOption(Option.MaxConcurrency, 3)
            .SetOption(Option.BackupEnabled, true)
            .AddListenerUpdateInfo((_, e) =>
                Console.WriteLine($"[Version] Found: {e.Version} | Size: {e.Size}"))
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[Download] {e.ProgressValue}% | {e.Speed}"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[Download] Complete: {e.Versions?.LastOrDefault()?.Version}"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[Error] {e.Message}"));
        await bootstrap.LaunchAsync();
    }
}
