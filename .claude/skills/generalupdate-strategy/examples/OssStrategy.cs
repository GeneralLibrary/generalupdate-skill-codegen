using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// 策略 2：OSS 对象存储更新
///
/// 适用场景：
/// - 没有后端服务，只有对象存储（AWS S3 / MinIO / 阿里云OSS / 华为OBS）
/// - 想以最低成本实现自动更新
/// - 不需要复杂的版本管理和升级追踪
///
/// 工作原理：
/// 1. Client 下载 versions.json（从 OSS 的固定路径）
/// 2. 比较客户端版本 vs versions.json 中的最新版本
/// 3. 有更新 → 直接从 OSS 下载 ZIP 包
/// 4. 启动 Upgrade 进程应用更新
///
/// OSS 上需要的文件：
///   your-bucket/
///   ├── versions.json           ← 版本清单，由发布工具生成
///   ├── v1.1.0.0/
///   │   └── update.zip
///   └── v1.2.0.0/
///       └── update.zip
///
/// versions.json 格式：
/// {
///   "versions": [
///     { "version": "1.1.0.0", "url": "https://bucket/v1.1.0.0/update.zip", "hash": "...", "size": 1048576 },
///     { "version": "1.2.0.0", "url": "https://bucket/v1.2.0.0/update.zip", "hash": "...", "size": 2097152 }
///   ]
/// }
///
/// NuGet: dotnet add package GeneralUpdate.Core
///
/// ⚠️ 已知问题（来自 Issue #485、#487）：
/// 1. OSS 模式不区分 MainApp 和 UpgradeApp 更新，两者的可用性总是同步的
/// 2. SSL 验证策略默认不覆盖文件下载请求
/// 3. UpgradeApp.exe 必须放在 update/ 子目录中
/// </summary>
public static class OssStrategy
{
    public static async Task RunAsync()
    {
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-storage.com/versions.json",   // OSS versions.json URL
                "your-secret-key")                          // 用于 IPC 加密
            .SetOption(Option.AppType, AppType.OssClient)   // 使用 OSS 客户端模式
            .SetOption(Option.MaxConcurrency, 3)
            .SetOption(Option.BackupEnabled, false)

            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"OSS 下载: {e.ProgressValue}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"OSS 版本 {e.Versions?.LastOrDefault()?.Version} 下载完成"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"OSS 错误: {e.Message}"));

        var result = await bootstrap.LaunchAsync();
        Console.WriteLine(result ? "更新完成" : "已是最新");
    }
}
