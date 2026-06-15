using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// Strategy 2: OSS (Object Storage Service) update.
/// No backend server required — uses S3/MinIO/Aliyun OSS directly.
///
/// How it works:
/// 1. Client downloads versions.json from OSS
/// 2. Compares client version vs latest in versions.json
/// 3. Downloads update ZIP directly from OSS
/// 4. Starts Upgrade process
///
/// OSS structure:
///   bucket/
///   +-- versions.json
///   +-- v1.1.0.0/update.zip
///   +-- v1.2.0.0/update.zip
///
/// NuGet: dotnet add package GeneralUpdate.Core
///
/// Known issues (#485, #487):
/// 1. OSS does not distinguish Main/Upgrade updates
/// 2. SSL validation does NOT cover file downloads
/// 3. Upgrade.exe must be in update/ subdirectory
/// </summary>
public static class OssStrategy
{
    public static async Task RunAsync()
    {
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource("https://your-storage.com/versions.json", "your-secret-key")
            .SetOption(Option.AppType, AppType.OssClient)
            .SetOption(Option.MaxConcurrency, 3)
            .SetOption(Option.BackupEnabled, false)
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[OSS] Download: {e.ProgressValue}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[OSS] Version {e.Versions?.LastOrDefault()?.Version} done"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[OSS] Error: {e.Message}"));

        var result = await bootstrap.LaunchAsync();
        Console.WriteLine(result ? "Update complete" : "Already latest");
    }
}
