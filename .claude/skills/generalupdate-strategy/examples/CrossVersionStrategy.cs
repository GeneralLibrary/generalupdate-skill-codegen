using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// Strategy 5: Cross-Version Package (CVP) — jump directly to latest.
/// Skips all intermediate versions, applies one CVP delta package.
///
/// Server: uses two full-package archives (v1 and v3) -> CrossVersionPacketBuilder
///   -> DiffPipeline.CleanAsync() -> CVP delta package
/// Client: downloads one CVP package -> applies directly -> done
///
/// CVP + chain fallback (v5.0+, #499):
/// Server returns both CVP and chain packages. Client tries CVP first,
/// auto-falls back to chain if it fails. No second server request needed.
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class CrossVersionStrategy
{
    public static async Task RunAsync()
    {
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource("https://your-server.com/api", "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)
            .SetOption(Option.BackupEnabled, true)
            .AddListenerUpdateInfo((_, e) =>
            {
                Console.WriteLine($"[CVP] Version: {e.Version} | Cross-version: {e.IsCrossVersion}");
                Console.WriteLine($"[CVP] Files: {e.FileCount} | Size: {e.Size}");
            })
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[CVP] Download: {e.ProgressValue}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
            {
                var last = e.Versions?.LastOrDefault();
                Console.WriteLine($"[CVP] Package done: {last?.Version} (CVP: {last?.IsCrossVersion})");
            })
            .AddListenerException((_, e) =>
                Console.WriteLine($"[CVP] Error: {e.Message}"));

        await bootstrap.LaunchAsync();
    }
}
