using GeneralUpdate.Bowl;
using GeneralUpdate.Core.Models;

/// <summary>
/// [Skill Generated] Bowl crash daemon integration.
/// Bowl monitors the main app after update. On crash it generates:
/// - MiniDump (.dmp)
/// - Crash report (.json)
/// - System diagnostics (event log / drivers / system info)
/// - Auto-restore from backup (optional)
///
/// NuGet: dotnet add package GeneralUpdate.Bowl
///
/// Notes:
/// - Bowl is fully tested on Windows only
/// - Rollback depends on BackupEnabled = true
/// - Keeps only the 3 most recent backups
/// - Requires procdump tool (Windows)
/// </summary>
public static class BowlIntegration
{
    public static async Task StartBowlAsync(string appPath, string installPath)
    {
        Console.WriteLine("[Bowl] Starting crash daemon...");

        var bowl = new Bowl();
        bowl.OnCrash += (crashReport) =>
        {
            Console.WriteLine($"[Bowl] Crash detected!");
            Console.WriteLine($"[Bowl] Reason: {crashReport.CrashReason}");
            Console.WriteLine($"[Bowl] Dump file: {crashReport.DumpFilePath}");

            if (crashReport.AutoRestore)
                Console.WriteLine("[Bowl] Restoring from backup...");
        };

        await bowl.LaunchAsync(new BowlOptions
        {
            TargetAppPath = appPath,
            InstallPath = installPath,
            AutoRestore = true,
            ReportOutputPath = Path.Combine(installPath, "CrashReports")
        });
    }
}
