using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// Upgrade process — launched by Client via IPC.
///
/// Responsibilities: Read IPC -> Apply updates -> Start main app -> Exit
/// Note: Upgrade process does NOT access network (all data pre-downloaded by Client).
/// Upgrade and Client MUST use the same AppSecretKey.
/// </summary>
Console.WriteLine("[Upgrade] Upgrade process started");

try
{
    var result = await new GeneralUpdateBootstrap()
        .SetOption(Option.AppType, AppType.Upgrade)
        .AddListenerProgress((_, e) =>
            Console.WriteLine($"[Upgrade] Processing: {e.FileName} - {e.ProgressValue}% ({e.Type})"))
        .AddListenerException((_, e) =>
            Console.WriteLine($"[Upgrade] Error: {e.Message}"))
        .LaunchAsync();

    Console.WriteLine(result
        ? "[Upgrade] Update complete, main app started"
        : "[Upgrade] No update needed, main app started");
}
catch (Exception ex)
{
    Console.WriteLine($"[Upgrade] Fatal error: {ex}");
    Environment.ExitCode = 1;
}
