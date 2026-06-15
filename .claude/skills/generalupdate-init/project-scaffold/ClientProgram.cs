using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

string updateUrl = args.Length > 0 ? args[0] : "https://your-server.com/api";
string secretKey = args.Length > 1 ? args[1] : "your-secret-key";

Console.WriteLine($"[Client] Starting version check: {updateUrl}");
Console.WriteLine($"[Client] Current version: {GetCurrentVersion()}");

var result = await new GeneralUpdateBootstrap()
    .SetSource(updateUrl, secretKey)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) =>
        Console.WriteLine($"[Client] Found version: {e.Version} ({e.Size} bytes)"))
    .AddListenerMultiDownloadStatistics((_, e) =>
        Console.WriteLine($"[Client] Download: {e.ProgressValue}% | {e.Speed}/s"))
    .AddListenerMultiDownloadCompleted((_, e) =>
        Console.WriteLine($"[Client] Version {e.Versions?.LastOrDefault()?.Version} downloaded"))
    .AddListenerMultiAllDownloadCompleted((_, e) =>
        Console.WriteLine("[Client] All downloads complete, starting Upgrade process"))
    .AddListenerException((_, e) =>
        Console.WriteLine($"[Client] Error: {e.Message}"))
    .LaunchAsync();

if (result)
    Console.WriteLine("[Client] Update complete, restarting...");
else
{
    Console.WriteLine("[Client] Already latest version");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

static string GetCurrentVersion()
{
    return System.Reflection.Assembly.GetEntryAssembly()
        ?.GetName()?.Version?.ToString(4) ?? "1.0.0.0";
}
