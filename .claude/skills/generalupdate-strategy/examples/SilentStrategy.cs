using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

/// <summary>
/// 静默后台更新策略
///
/// 适用于用户长期不关闭应用的场景（如桌面工具、监控面板）。
/// GeneralUpdate 在检测到更新后自动后台下载，下次启动时应用新版本。
///
/// v10.5.0-beta.4 通过 SetOption(Option.Silent, true) 启用静默模式。
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class SilentStrategy
{
    public static async Task RunAsync()
    {
        await new GeneralUpdateBootstrap()
            .SetSource(
                updateUrl: "https://your-server.com/api",
                appSecretKey: "your-secret-key")
            .SetOption(Option.Silent, true)
            .SetOption(Option.SilentPollIntervalMinutes, 60)
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
