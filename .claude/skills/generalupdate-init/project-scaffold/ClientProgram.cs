using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

string updateUrl = args.Length > 0 ? args[0] : "https://your-server.com/api";
string secretKey = args.Length > 1 ? args[1] : "your-secret-key";

Console.WriteLine($"[Client] 启动版本检查: {updateUrl}");
Console.WriteLine($"[Client] 当前版本: {GetCurrentVersion()}");

var result = await new GeneralUpdateBootstrap()
    .SetSource(updateUrl, secretKey)
    .SetOption(Option.AppType, AppType.Client)
    .AddListenerUpdateInfo((_, e) =>
        Console.WriteLine($"[Client] 发现新版本: {e.Version} ({e.Size} bytes)"))
    .AddListenerMultiDownloadStatistics((_, e) =>
        Console.WriteLine($"[Client] 下载进度: {e.ProgressValue}% | {e.Speed}/s"))
    .AddListenerMultiDownloadCompleted((_, e) =>
        Console.WriteLine($"[Client] 版本 {e.Versions?.LastOrDefault()?.Version} 下载完成"))
    .AddListenerMultiAllDownloadCompleted((_, e) =>
        Console.WriteLine("[Client] 全部下载完成，即将启动升级程序"))
    .AddListenerException((_, e) =>
        Console.WriteLine($"[Client] 错误: {e.Message}"))
    .LaunchAsync();

if (result)
{
    Console.WriteLine("[Client] 更新完成，应用重启中...");
}
else
{
    Console.WriteLine("[Client] 已是最新版本");
    Console.WriteLine("按任意键退出...");
    Console.ReadKey();
}

static string GetCurrentVersion()
{
    var version = System.Reflection.Assembly.GetEntryAssembly()
        ?.GetName()?.Version;
    return version?.ToString() ?? "1.0.0.0";
}
