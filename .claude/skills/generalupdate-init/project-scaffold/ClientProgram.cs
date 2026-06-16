using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

string updateUrl = args.Length > 0 ? args[0] : "https://your-server.com/api";
string secretKey = args.Length > 1 ? args[1] : "your-secret-key";

Console.WriteLine($"[Client] 启动版本检查: {updateUrl}");

var config = new UpdateRequest
{
    UpdateUrl = updateUrl,
    AppSecretKey = secretKey,
    MainAppName = "MyApp.exe",
    ClientVersion = GetCurrentVersion(),
    ProductId = "my-product-001",
    InstallPath = "."
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) =>
        Console.WriteLine($"[Client] 发现 {e.Info?.Body?.Count ?? 0} 个版本"))
    .AddListenerMultiDownloadStatistics((_, e) =>
        Console.WriteLine($"[Client] 下载进度: {e.ProgressPercentage}% | {e.Speed}"))
    .AddListenerMultiDownloadCompleted((_, e) =>
        Console.WriteLine($"[Client] 版本 {e.Version} 下载完成"))
    .AddListenerMultiAllDownloadCompleted((_, e) =>
        Console.WriteLine("[Client] 全部下载完成"))
    .AddListenerException((_, e) =>
        Console.WriteLine($"[Client] 错误: {e.Message}"))
    .LaunchAsync();

Console.WriteLine("[Client] 更新流程完成，进程即将退出/继续执行");

static string GetCurrentVersion()
{
    var version = System.Reflection.Assembly.GetEntryAssembly()
        ?.GetName()?.Version;
    return version?.ToString() ?? "1.0.0.0";
}
