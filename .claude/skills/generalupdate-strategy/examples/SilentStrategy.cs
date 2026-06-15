using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// Strategy 3: Silent background update.
/// For long-running apps that should update without disturbing the user.
/// Updates are downloaded in the background and applied when the app exits.
///
/// Flow: Background polling -> download -> IPC -> ProcessExit -> Upgrade
///
/// NuGet: dotnet add package GeneralUpdate.Core
///
/// Known issues (Issues #484, #471, #443):
/// 1. ProcessExit not guaranteed (FailFast/TerminateProcess/Ctrl+C)
///    Fix: Call TryLaunchUpgrade() explicitly on app close
/// 2. manifest.json defaults can block auto-discovery
///    Fix: Fill version fields in manifest
/// 3. PatchMiddleware throws without DiffPipeline injection
///    Fix: Set PatchEnabled = false for silent mode
/// 4. Auto-restarts app after update (#443)
///    Fix: Configure SilentAutoRestart = false
/// </summary>
public class SilentStrategy : IDisposable
{
    private GeneralUpdateBootstrap? _bootstrap;
    private SilentPollOrchestrator? _orchestrator;

    public async Task RunAsync()
    {
        _bootstrap = new GeneralUpdateBootstrap()
            .SetSource("https://your-server.com/api", "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)
            .SetOption(Option.Silent, true)
            .SetOption(Option.SilentPollIntervalMinutes, 60)
            .SetOption(Option.PatchEnabled, false)    // Avoid issue #471
            .SetOption(Option.BackupEnabled, true)
            .AddListenerUpdateInfo((_, e) =>
                Console.WriteLine($"[Silent] New version: {e.Version}"))
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[Silent] Downloading: {e.ProgressValue}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[Silent] Version {e.Versions?.LastOrDefault()?.Version} ready"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[Silent] Error: {e.Message}"));

        var result = await _bootstrap.LaunchAsync();
        _orchestrator = _bootstrap.SilentOrchestrator;

        if (_orchestrator != null)
            _orchestrator.HasPreparedUpdate += () =>
                Console.WriteLine("[Silent] Update ready, will install on exit");

        await RunApplicationAsync();
    }

    /// <summary>Call this on app close instead of relying on ProcessExit.</summary>
    public void OnAppClosing()
    {
        try { _orchestrator?.TryLaunchUpgrade(); }
        catch (Exception ex) { Console.WriteLine($"[Silent] Launch failed: {ex.Message}"); }
    }

    private async Task RunApplicationAsync()
    {
        Console.WriteLine("[Silent] App running, updates downloading in background...");
        await Task.Delay(TimeSpan.FromHours(8));
    }

    public void Dispose() => OnAppClosing();
}
