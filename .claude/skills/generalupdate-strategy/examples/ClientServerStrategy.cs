using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// 策略 1：标准客户端-服务端更新
///
/// 适用场景：
/// - 已部署 GeneralSpacestation 或兼容的后端服务
/// - 需要精确控制更新包的发布
/// - 需要升级状态跟踪和上报
///
/// 后端要求：
/// - POST /Upgrade/Verification — 版本验证
/// - (可选) POST /Upgrade/Report — 状态上报
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class ClientServerStrategy
{
    public static async Task RunAsync()
    {
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-server.com/api",
                "your-32-char-secret-key")
            .SetOption(Option.AppType, AppType.Client)

            // 可选配置
            .SetOption(Option.MaxConcurrency, 3)
            .SetOption(Option.BackupEnabled, true)
            .SetOption(Option.PatchEnabled, false)
            .SetOption(Option.DownloadTimeout, 60)

            // 事件
            .AddListenerUpdateInfo((_, e) =>
                Console.WriteLine($"发现版本: {e.Version} | 大小: {e.Size}"))
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"进度: {e.ProgressValue}% | {e.Speed}"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"下载完成: {e.Versions?.LastOrDefault()?.Version}"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"错误: {e.Message}"));

        await bootstrap.LaunchAsync();
    }
}
