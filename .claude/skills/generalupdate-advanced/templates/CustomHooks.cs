using GeneralUpdate.Core.Hooks;
using GeneralUpdate.Core.Models;

/// <summary>
/// [Skill Generated] Custom lifecycle hooks.
/// Implements IUpdateHooks for full lifecycle control.
/// All methods have default implementations (return null/true) — override only what you need.
///
/// Usage:
///   .Hooks<MyCustomHooks>()
/// </summary>
public class MyCustomHooks : IUpdateHooks
{
    /// <summary>Called before update starts. Return false to abort.</summary>
    public async Task<bool> OnBeforeUpdateAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] Update starting: {context.CurrentVersion} -> {context.LastVersion}");

        // Check disk space
        var drive = new DriveInfo(Path.GetPathRoot(context.InstallPath)!);
        if (drive.AvailableFreeSpace < 100 * 1024 * 1024)
        {
            Console.WriteLine("[Hooks] Insufficient disk space, aborting");
            return false;
        }

        return true;
    }

    /// <summary>Called after download completes (Client process).</summary>
    public async Task OnDownloadCompletedAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] Download complete: {context.LastVersion}");
        await Task.CompletedTask;
    }

    /// <summary>Called after update applies (Upgrade process). Clean up temp files, migrate configs.</summary>
    public async Task OnAfterUpdateAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] Update complete: {context.LastVersion}");
        var tempDir = context.UpdatePath;
        if (Directory.Exists(tempDir))
            try { Directory.Delete(tempDir, true); } catch { }
        await Task.CompletedTask;
    }

    /// <summary>Called on update error. Log the error, notify admin, trigger rollback.</summary>
    public async Task OnUpdateErrorAsync(UpdateContext context, Exception exception)
    {
        Console.WriteLine($"[Hooks] Update failed: {exception.Message}");
        File.WriteAllText(Path.Combine(context.InstallPath, "update_error.log"),
            $"[{DateTime.UtcNow}] {exception}");
        await Task.CompletedTask;
    }

    /// <summary>Called before starting the main app. Return false to prevent launch.</summary>
    public async Task<bool> OnBeforeStartAppAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] Preparing to launch: {context.MainAppName}");

        // Set executable permissions on Linux/macOS
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var appPath = Path.Combine(context.InstallPath, context.MainAppName ?? "");
            if (File.Exists(appPath))
                await UnixPermissionHooks.SetExecutablePermissionAsync(appPath);
        }

        return true;
    }
}
