using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Common.Download;

/// <summary>
/// GeneralUpdate 完整集成示例 — 包含所有配置选项和事件监听
///
/// 覆盖：
/// - Configinfo 完整配置
/// - 全部 6 个事件监听器
/// - 升级场景理解
/// - 错误处理
/// - Upgrade 进程配置
///
/// 针对 NuGet v10.4.6 稳定版
/// </summary>
public static class FullIntegration
{
    public static async Task RunAsync()
    {
        try
        {
            // ========== 1. 构建配置 ==========
            var config = new Configinfo
            {
                // --- 必填 ---
                UpdateUrl = "https://your-server.com/Upgrade/Verification",
                AppSecretKey = "your-32-char-secret-key-here!",

                // --- 应用信息 ---
                AppName = "MyApp.exe",
                MainAppName = "MyApp.exe",
                ClientVersion = "1.0.0.0",         // ⚠️ 4 段式
                ProductId = "my-product-001",
                InstallPath = AppDomain.CurrentDomain.BaseDirectory,

                // --- 可选 ---
                ReportUrl = "https://your-server.com/Upgrade/Report",
                UpdateLogUrl = "https://your-server.com/Upgrade/Log",
            };

            // ========== 2. 配置 Bootstrap ==========
            var bootstrap = await new GeneralUpdateBootstrap()
                .SetConfig(config)

                // 事件：版本发现
                .AddListenerUpdateInfo(OnUpdateInfo)
                // 事件：批量下载进度
                .AddListenerMultiDownloadStatistics(OnDownloadStats)
                // 事件：每版本下载完成
                .AddListenerMultiDownloadCompleted(OnDownloadCompleted)
                // 事件：全部下载完成
                .AddListenerMultiAllDownloadCompleted(OnAllDownloadCompleted)
                // 事件：下载错误
                .AddListenerMultiDownloadError(OnDownloadError)
                // 事件：异常
                .AddListenerException(OnException)

                // ========== 3. 执行 ==========
                .LaunchAsync();

            // LaunchAsync 返回 bootstrap 实例
            // 有更新 → 进程退出由 Upgrade 进程重启
            // 无更新 → 继续执行
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[严重] 更新异常: {ex.Message}");
        }
    }

    // ========== 事件处理函数 ==========

    private static void OnUpdateInfo(object? sender, UpdateInfoEventArgs e)
    {
        if (e.Info?.Body != null)
        {
            Console.WriteLine($"[版本发现] 版本数: {e.Info.Body.Count}");
            foreach (var v in e.Info.Body)
                Console.WriteLine($"  ├─ {v.Version} (AppType={v.AppType}) {v.Name ?? ""}");
        }
    }

    private static void OnDownloadStats(object? sender, MultiDownloadStatisticsEventArgs e)
    {
        Console.Write($"\r[下载] {e.ProgressPercentage:F0}% | {e.Speed} | 剩余 {e.Remaining:hh\\:mm\\:ss}");
    }

    private static void OnDownloadCompleted(object? sender, MultiDownloadCompletedEventArgs e)
    {
        Console.WriteLine($"\n[下载完成] 版本: {e.Version} (IsComplated={e.IsComplated})");
    }

    private static void OnAllDownloadCompleted(object? sender, MultiAllDownloadCompletedEventArgs e)
    {
        Console.WriteLine($"[全部完成] 成功: {e.IsAllDownloadCompleted}, 失败版本数: {e.FailedVersions?.Count ?? 0}");
    }

    private static void OnDownloadError(object? sender, MultiDownloadErrorEventArgs e)
    {
        Console.WriteLine($"\n[下载错误] 版本: {e.Version} — {e.Exception?.Message}");
    }

    private static void OnException(object? sender, ExceptionEventArgs e)
    {
        Console.WriteLine($"[异常] {e.Message}");
    }
}

/// <summary>
/// Upgrade 进程配置（供 UpgradeApp.exe 使用）
/// </summary>
public static class UpgradeProcessIntegration
{
    public static async Task RunAsync()
    {
        Console.WriteLine("[Upgrade] 升级程序启动 — 从 IPC 读取配置");

        try
        {
            // Upgrade 模式不需要 SetConfig — 配置由 IPC 传递
            await new GeneralUpdateBootstrap()
                .AddListenerException((_, e) =>
                    Console.WriteLine($"[Upgrade] 错误: {e.Message}"))
                .LaunchAsync();

            Console.WriteLine("[Upgrade] 更新完成，主程序已启动");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Upgrade] 严重错误: {ex}");
            Environment.ExitCode = 1;
        }
    }
}
