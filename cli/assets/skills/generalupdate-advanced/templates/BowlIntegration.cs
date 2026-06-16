using System;
using System.Threading.Tasks;
using GeneralUpdate.Bowl;

/// <summary>
/// Bowl crash daemon integration.
///
/// Bowl monitors whether the main application starts normally after an upgrade.
/// If a crash is detected, it captures a dump, exports diagnostics,
/// and optionally restores the previous version from backup.
///
/// NuGet: dotnet add package GeneralUpdate.Bowl --version 10.5.0-beta.4
/// Note: Reference only GeneralUpdate.Bowl (it transitively includes Core).
/// Do NOT reference GeneralUpdate.Core separately when using Bowl.
///
/// Platform prerequisites:
///   - Windows: Sysinternals procdump.exe is auto-bundled via Bowl NuGet package
///   - Linux:   Requires procdump installed (sudo apt install procdump)
/// </summary>
public static class BowlIntegration
{
    public static async Task RunBowlAsync()
    {
        // Configure the surveillance context
        var context = new BowlContext
        {
            // Process to monitor (name or PID)
            ProcessNameOrId = "MyApp.exe",

            // Backup directory path (the version that was running before upgrade)
            TargetPath = @"C:\Program Files\MyApp",

            // Version string, used to name dump/crash files
            ExtendedField = "1.0.0.1",

            // Generated dump file path
            DumpFileName = "1.0.0.1_fail.dmp",

            // Generated crash report file path
            FailFileName = "1.0.0.1_fail.json",

            // Where dump/crash files will be written
            FailDirectory = @"C:\Program Files\MyApp\fail\1.0.0.1",

            // Backup location (the previous version's backup that can be restored)
            BackupDirectory = @"C:\Program Files\MyApp\1.0.0.0",

            // "Upgrade": integrated with update pipeline, auto-restores on crash
            // "Normal":  standalone monitoring, no restore
            WorkModel = "Upgrade",

            // Auto-restore the previous version on crash (only in "Upgrade" mode)
            AutoRestore = true,

            // Dump type: Full (0), Mini (1), or Heap (2)
            DumpType = DumpType.Full,

            // Timeout for child process (procdump), default 30s
            TimeoutMs = 30_000,

            // Optional: crash callback for custom handling (logging, telemetry, etc.)
            // OnCrash = async (crashInfo, ct) => { ... },
        };

        // Apply sensible defaults (Normalize fills in TimeoutMs, WorkModel, DumpType if zero)
        context = context.Normalize();

        // Start surveillance. This blocks until the monitored process exits.
        var bowl = new Bowl();
        BowlResult result = await bowl.LaunchAsync(context);

        Console.WriteLine($"""
            [Bowl] 监控完成
              Success:      {result.Success}
              ExitCode:     {result.ExitCode}
              DumpCaptured: {result.DumpCaptured}
              DumpFilePath: {result.DumpFilePath}
              Restored:     {result.Restored}
            """);
    }
}
