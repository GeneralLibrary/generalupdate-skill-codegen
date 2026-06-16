using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

/// <summary>
/// 标准客户端-服务端更新策略
///
/// 适用于已部署 GeneralSpacestation 或兼容后端的应用。
/// 后端要求：
/// - POST /Upgrade/Verification — 版本验证
/// - (可选) POST /Upgrade/Report — 状态上报
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// ⚠️ 针对 NuGet v10.5.0-beta.4
/// </summary>
public static class ClientServerStrategy
{
    public static async Task RunAsync()
    {
        var config = new UpdateRequest
        {
            UpdateUrl = "https://your-server.com/api",
            AppSecretKey = "your-32-char-secret-key",
            MainAppName = "MyApp.exe",
            ClientVersion = "1.0.0.0",
            ProductId = "my-product-001",
            InstallPath = ".",
        };

        await new GeneralUpdateBootstrap()
            .SetConfig(config)
            .AddListenerUpdateInfo((_, e) =>
                Console.WriteLine($"[版本发现] 发现 {e.Info?.Body?.Count ?? 0} 个版本"))
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"进度: {e.ProgressPercentage}% | {e.Speed}"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"下载完成: {e.Version} (IsCompleted={e.IsCompleted})"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"错误: {e.Message}"))
            .LaunchAsync();
    }
}
