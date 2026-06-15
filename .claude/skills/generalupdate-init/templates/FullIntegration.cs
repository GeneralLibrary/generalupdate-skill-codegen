using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// GeneralUpdate 完整集成示例 — 包含所有配置选项、事件监听和 4 种场景处理
///
/// 覆盖：
/// - 三种配置方式（SetSource / SetConfig / LoadFromConfiguration）
/// - UpdateRequest 所有字段
/// - 全部 7+ 事件监听器
/// - HTTP 认证（HMAC / Basic / Bearer）
/// - 4 大更新场景的业务含义
/// - 错误处理与回退
/// - Upgrade 进程配置
/// - appsettings.json 配置分离
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class FullIntegration
{
    /// <summary>
    /// 方式 1：SetConfig + 显式 UpdateRequest（推荐生产环境）
    /// </summary>
    public static async Task<bool> RunWithExplicitConfigAsync()
    {
        try
        {
            // ========== 1. 构建配置 ==========
            var request = new UpdateRequest
            {
                // --- 必填 ---
                UpdateUrl = "https://your-server.com/Upgrade/Verification",
                AppSecretKey = "your-32-char-secret-key-here!", // IPC 加密 + HTTP 认证使用同一个 Key

                // --- 应用信息（也可由 manifest.json 自动发现）---
                InstallPath = AppDomain.CurrentDomain.BaseDirectory,
                UpdatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update"),
                ClientVersion = "1.0.0.0",       // ⚠️ 必须 4 段式，"1.0" 会触发 ArgumentException
                MainAppName = "MyApp.exe",        // ⚠️ 必须包含扩展名
                UpdateAppName = "UpgradeApp.exe",
                ProductId = "my-product-001",

                // --- HTTP 认证（选其一）---
                // HMAC：设置 AppSecretKey 即自动生效（与 GeneralSpacestation 默认方案）
                // Basic：取消下面两行的注释
                // BasicUsername = "my-username",
                // BasicPassword = "my-password",

                // --- 可选功能地址 ---
                ReportUrl = "https://your-server.com/Upgrade/Report",
                UpdateLogUrl = "https://your-server.com/Upgrade/Log",
            };

            // ========== 2. 配置 Bootstrap ==========
            var bootstrap = new GeneralUpdateBootstrap()
                .SetConfig(request)

                // --- 运行时选项 ---
                .SetOption(Option.AppType, AppType.Client)
                .SetOption(Option.MaxConcurrency, 3)            // 并行下载（默认 3）
                .SetOption(Option.PatchEnabled, false)          // 差分更新（需安装 Differential 包）
                .SetOption(Option.BackupEnabled, true)           // v5+ 默认关闭，需手动启用
                .SetOption(Option.DownloadTimeout, 60)          // HTTP 超时
                .SetOption(Option.EnableResume, true)            // 断点续传
                .SetOption(Option.VerifyChecksum, true)          // 下载后 SHA256 校验
                .SetOption(Option.MaxRetryCount, 3)             // 下载重试
                .AddListenerUpdateInfo(OnUpdateInfo)
                .AddListenerMultiDownloadStatistics(OnDownloadStats)
                .AddListenerMultiDownloadCompleted(OnDownloadCompleted)
                .AddListenerMultiAllDownloadCompleted(OnAllDownloadCompleted)
                .AddListenerMultiDownloadError(OnDownloadError)
                .AddListenerProgress(OnProgress)
                .AddListenerException(OnException);

            // ========== 3. 执行 ==========
            var result = await bootstrap.LaunchAsync();

            // result:
            //   true  = 更新完成，当前进程即将退出（由 Upgrade 进程重新启动主程序）
            //   false = 已是最新版本，当前进程将继续正常运行
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[严重] 更新流程异常: {ex.Message}");
            Console.WriteLine($"[严重] {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 方式 2：SetSource + manifest.json 零配置（快速原型）
    /// </summary>
    public static async Task<bool> RunWithMinimalConfigAsync()
    {
        // ⚠️ 前提：发布目录必须有 generalupdate.manifest.json
        return await new GeneralUpdateBootstrap()
            .SetSource("https://your-server.com/api/", "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)
            .LaunchAsync();
    }

    /// <summary>
    /// 方式 3：LoadFromConfiguration（appsettings.json 配置分离）
    /// </summary>
    public static async Task<bool> RunWithAppSettingsAsync()
    {
        // appsettings.json 内容见 reference.md

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        return await new GeneralUpdateBootstrap()
            .LoadFromConfiguration(config.GetSection("GeneralUpdate"))
            .SetOption(Option.AppType, AppType.Client)
            .LaunchAsync();
    }

    // ========== 事件处理函数 ==========

    /// <summary>版本验证结果（决定是否有更新）</summary>
    private static void OnUpdateInfo(object? sender, UpdateInfoEventArgs e)
    {
        Console.WriteLine($"[版本发现] 新版本: {e.Version}");
        Console.WriteLine($"[版本发现] 文件数: {e.FileCount}");
        Console.WriteLine($"[版本发现] 总大小: {e.Size} bytes");

        if (e.Info?.Body != null)
        {
            foreach (var v in e.Info.Body)
            {
                var type = v.IsCrossVersion ? "跨版本" : (v.FromVersion == null ? "全量" : "增量");
                Console.WriteLine($"  ├─ {v.Version} [{type}] {v.Name}");
                Console.WriteLine($"  │  Hash: {v.Hash?[..16]}...");
                Console.WriteLine($"  │  AppType: {v.AppType} (0=Client, 1=Upgrade)");
                // ⚠️ AppType 决定场景判断（#465），Client 包 → HasMainUpdate，Upgrade 包 → HasUpgradeUpdate
            }
        }
    }

    /// <summary>批量下载实时进度</summary>
    private static void OnDownloadStats(object? sender, MultiDownloadStatisticsEventArgs e)
    {
        // e.ProgressValue: 0-100
        // e.Speed: 格式如 "2.5 MB/s"
        // e.Remaining: TimeSpan
        // e.Version: VersionEntry?（当前下载的版本）
        Console.Write($"\r[下载] {e.ProgressPercentage:F0}% | {e.Speed}/s | " +
            $"剩余 {e.Remaining:hh\\:mm\\:ss}");
    }

    /// <summary>单个版本下载完成</summary>
    private static void OnDownloadCompleted(object? sender, MultiDownloadCompletedEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine($"[下载完成] 版本: {e.Versions?.LastOrDefault()?.Version}");
        Console.WriteLine($"[下载完成] 状态: {(e.IsCompleted ? "✓ 成功" : "✗ 部分失败")}");
    }

    /// <summary>全部版本下载完成</summary>
    private static void OnAllDownloadCompleted(object? sender, MultiAllDownloadCompletedEventArgs e)
    {
        // 此事件触发后，Client 将：
        // 1. 如果只有 Upgrade 更新 → 原地解压 Upgrade 包
        // 2. 如果只有 Main 更新 → 写入 IPC → 启动 Upgrade 进程
        // 3. 两者都有 → 先解压 Upgrade 包 → IPC → 启动 Upgrade 进程
        Console.WriteLine("[全部完成] 准备进入下一阶段");

        if (e.IsUpgradeUpdate && e.IsMainUpdate)
            Console.WriteLine("[场景] Both — 更新升级程序 + 主程序");
        else if (e.IsMainUpdate)
            Console.WriteLine("[场景] MainOnly — 只更新主程序");
        else if (e.IsUpgradeUpdate)
            Console.WriteLine("[场景] UpgradeOnly — 只更新升级程序");
    }

    /// <summary>单文件下载错误</summary>
    private static void OnDownloadError(object? sender, MultiDownloadErrorEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine($"[下载错误] 文件: {e.FileName}");
        Console.WriteLine($"[下载错误] 原因: {e.Exception?.Message}");
        // 不会中断整体流程，继续下载其他文件
    }

    /// <summary>单文件处理进度（含解压、哈希校验、补丁应用）</summary>
    private static void OnProgress(object? sender, ProgressEventArgs e)
    {
        // e.Type: "Download" / "Decompress" / "Hash" / "Patch"
        if (e.Progress != null)
        {
            var p = e.Progress;
            Console.WriteLine($"[处理] {p.AssetName}: {p.Percentage:F0}% ({p.Status})");
        }
    }

    /// <summary>更新异常</summary>
    private static void OnException(object? sender, ExceptionEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine($"[异常] {e.Message}");
        // ⚠️ Exception 事件通知不中断流程，需要自行处理
    }
}

/// <summary>
/// Upgrade 进程配置（供 UpgradeApp.exe 使用）
///
/// ⚠️ UpgradeApp.exe 和 MyApp.exe 必须使用相同的 AppSecretKey！
/// ⚠️ UpgradeApp.exe 必须随首个版本一起发布！
/// </summary>
public static class UpgradeProcessIntegration
{
    public static async Task RunAsync()
    {
        Console.WriteLine("[Upgrade] 升级程序启动 — 从 IPC 读取配置");

        try
        {
            // Upgrade 模式不需要 SetSource/SetConfig
            // 所有配置通过 IPC 文件从 Client 进程传递
            await new GeneralUpdateBootstrap()
                .SetOption(Option.AppType, AppType.Upgrade)
                .AddListenerProgress((_, e) =>
                {
                    Console.WriteLine($"[Upgrade] {e.FileName}: {e.ProgressValue}%");
                })
                .AddListenerException((_, e) =>
                {
                    Console.WriteLine($"[Upgrade] 错误: {e.Message}");
                })
                .LaunchAsync();

            Console.WriteLine("[Upgrade] 更新完成，主程序已启动");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Upgrade] 严重错误: {ex}");
            // Upgrade 进程失败 → 记录日志后退出
            // Client 下次启动时 Since 版本未变，会重新下载并重试
            Environment.ExitCode = 1;
        }
    }
}
