using GeneralUpdate.Core.Strategy;
using GeneralUpdate.Core.Pipeline;

/// <summary>
/// [Skill Generated] Custom platform update strategy.
/// Completely replaces default strategies (WindowsStrategy / LinuxStrategy / MacStrategy).
///
/// Usage:
///   .Strategy<MyCustomStrategy>()
/// </summary>
public class MyCustomStrategy : AbstractStrategy
{
    public override async Task ExecuteAsync(UpdateContext context)
    {
        Console.WriteLine("[CustomStrategy] Executing custom update strategy");

        // 1. Pre-update check
        if (await Hooks.SafeOnBeforeUpdateAsync(context) == false)
        {
            Console.WriteLine("[CustomStrategy] Pre-check failed, aborting");
            return;
        }

        // 2. Process each version through the pipeline
        foreach (var version in context.UpdateVersions)
        {
            Console.WriteLine($"[CustomStrategy] Processing version: {version.Version}");

            var pipeline = new PipelineBuilder(context)
                .UseMiddleware<HashMiddleware>()
                .UseMiddleware<CompressMiddleware>()
                .Build();

            await pipeline.ExecuteAsync(context, version);
            Console.WriteLine($"[CustomStrategy] Version {version.Version} done");
        }

        // 3. Post-update
        await Hooks.SafeOnAfterUpdateAsync(context);

        // 4. Start main app
        await StartAppAsync(context);
    }

    public override async Task StartAppAsync(UpdateContext context)
    {
        Console.WriteLine("[CustomStrategy] Starting main app");
        var appPath = Path.Combine(context.InstallPath, context.MainAppName ?? "MyApp.exe");
        if (!File.Exists(appPath))
            throw new FileNotFoundException($"App not found: {appPath}");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = appPath,
            WorkingDirectory = context.InstallPath,
            UseShellExecute = true
        });

        if (process == null)
            throw new InvalidOperationException($"Failed to start: {appPath}");

        Console.WriteLine($"[CustomStrategy] App started (PID: {process.Id})");
    }

    public override async Task ExecuteAsync(UpdateContext context, string pipeHandle)
    {
        await ExecuteAsync(context);
    }
}
