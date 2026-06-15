using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// GeneralUpdate 完整集成示例 — 包含所有配置选项和事件监听
///
/// 适合需要精细控制的场景，提供：
/// - 完整 UpdateRequest 配置
/// - 所有 Option 设置
/// - 全部 7 种事件监听
/// - 错误处理
/// </summary>
public static class FullIntegration
{
    public static async Task RunAsync()
    {
        try
        {
            // ========== 1. 配置 ==========
            var request = new UpdateRequest
            {
                // --- 必需字段 ---
                UpdateUrl = "https://your-server.com/Upgrade/Verification",
                AppSecretKey = "your-32-char-secret-key-here!",

                // --- 应用信息（也可由 manifest.json 自动发现）---
                InstallPath = AppDomain.CurrentDomain.BaseDirectory,
                UpdatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update"),
                ClientVersion = "1.0.0.0",
                MainAppName = "MyApp.exe",
                UpdateAppName = "UpgradeApp.exe",
                ProductId = "my-product-001",

                // --- HTTP 认证（按需选用）---
                ReportUrl = "https://your-server.com/Upgrade/Report",
                // BasicUsername = "user",
                // BasicPassword = "pass",
            };

            // ========== 2. Bootstrap 配置 ==========
            var bootstrap = new GeneralUpdateBootstrap()
                .SetConfig(request)

                // AppType: Client (主程序) | Upgrade (升级程序)
                //          OssClient | OssUpgrade (对象存储模式)
                .SetOption(Option.AppType, AppType.Client)

                // 并行下载数（默认 3）
                .SetOption(Option.MaxConcurrency, 3)

                // 是否启用差分更新（需安装 GeneralUpdate.Differential）
                .SetOption(Option.PatchEnabled, true)

                // 压缩格式（默认 Zip，可选 Brotli / GZip）
                .SetOption(Option.Format, CompressionFormat.Zip)
                .SetOption(Option.Encoding, CompressionEncoding.Default)

                // 下载超时（秒）
                .SetOption(Option.DownloadTimeout, 60)

                // 启用备份（v5.0+ 默认关闭）
                .SetOption(Option.BackupEnabled, true)

                // 静默模式（后台下载，退出时更新）
                .SetOption(Option.Silent, false);

            // ========== 3. 事件监听 ==========
            // 3a. 版本验证结果
            bootstrap.AddListenerUpdateInfo((sender, e) =>
            {
                Console.WriteLine($"[更新信息] 版本: {e.Version}");
                Console.WriteLine($"[更新信息] 文件数: {e.FileCount}");
                Console.WriteLine($"[更新信息] 总大小: {e.Size} bytes");
            });

            // 3b. 多文件下载进度
            bootstrap.AddListenerMultiDownloadStatistics((sender, e) =>
            {
                Console.WriteLine($"[下载] {e.ProgressValue}% | " +
                    $"{e.Speed}/s | 剩余 {e.Remaining} | " +
                    $"版本 {e.Version?.Version}");
            });

            // 3c. 每个版本下载完成
            bootstrap.AddListenerMultiDownloadCompleted((sender, e) =>
            {
                var lastVer = e.Versions?.LastOrDefault();
                Console.WriteLine($"[下载完成] 版本 {lastVer?.Version}");
            });

            // 3d. 全部下载完成
            bootstrap.AddListenerMultiAllDownloadCompleted((sender, e) =>
            {
                Console.WriteLine("[全部下载完成] 准备启动升级程序");
            });

            // 3e. 单个文件下载错误
            bootstrap.AddListenerMultiDownloadError((sender, e) =>
            {
                Console.WriteLine($"[下载错误] {e.FileName}: {e.Exception?.Message}");
            });

            // 3f. 单文件进度（含解压/补丁）
            bootstrap.AddListenerProgress((sender, e) =>
            {
                Console.WriteLine($"[处理] {e.FileName} — " +
                    $"{e.ProgressValue}% ({e.Type})");
            });

            // 3g. 异常
            bootstrap.AddListenerException((sender, e) =>
            {
                Console.WriteLine($"[异常] {e.Message}");
            });

            // ========== 4. 启动 ==========
            var result = await bootstrap.LaunchAsync();

            if (result)
            {
                Console.WriteLine("[成功] 更新完成，应用即将重启");
                // 注意：这里不会执行到，因为 LaunchAsync 在当前进程退出前
                // 启动 Upgrade 进程，然后当前进程退出
            }
            else
            {
                Console.WriteLine("[信息] 已是最新版本，无需更新");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[严重] 更新流程异常: {ex}");
        }
    }
}

/// <summary>
/// AppType 枚举说明
///
/// Client       = 0 — 主应用程序（负责版本验证、下载、IPC、启动升级器）
/// Upgrade      = 1 — 升级程序（负责备份、替换文件、启动主程序）
/// OssClient    = 2 — 对象存储客户端（从 S3/MinIO/OSS 读取 versions.json）
/// OssUpgrade   = 3 — 对象存储升级端
/// </summary>
public enum AppType
{
    Client = 0,
    Upgrade = 1,
    OssClient = 2,
    OssUpgrade = 3
}
