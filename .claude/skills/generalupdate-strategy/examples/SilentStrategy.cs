using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Common.Download;

/// <summary>
/// 静默后台更新策略
///
/// 适用于用户长期不关闭应用的场景（如桌面工具、监控面板）。
/// GeneralUpdate 在检测到更新后自动后台下载，下次启动时应用新版本。
///
/// ⚠️ 注意：v10.4.6 稳定版没有 SilentPollOrchestrator 或 SetOption API。
/// 静默行为由配置和启动参数控制。应用关闭时自动触发升级。
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class SilentStrategy
{
    public static async Task RunAsync()
    {
        var config = new Configinfo
        {
            UpdateUrl = "https://your-server.com/api",
            AppSecretKey = "your-secret-key",
            AppName = "MyApp.exe",
            MainAppName = "MyApp.exe",
            ClientVersion = "1.0.0.0",
            ProductId = "my-product-001",
            InstallPath = ".",
        };

        await new GeneralUpdateBootstrap()
            .SetConfig(config)
            .AddListenerUpdateInfo((_, e) =>
                Console.WriteLine($"[静默] 发现 {e.Info?.Body?.Count ?? 0} 个版本"))
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[静默] 后台下载: {e.ProgressPercentage}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[静默] 版本 {e.Version} 就绪"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[静默] 错误: {e.Message}"))
            .LaunchAsync();
    }
}
