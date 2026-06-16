using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Common.Download;

/// <summary>
/// OSS 对象存储更新策略
///
/// 适用于没有后端服务，只有对象存储的场景（AWS S3 / MinIO / 阿里云OSS）。
/// 工作原理：读取 versions.json 版本清单 → 比较版本 → 下载 ZIP → 更新
///
/// OSS 上需要的文件：
///   your-bucket/
///   ├── versions.json           ← 版本清单
///   └── v1.1.0.0/
///       └── update.zip
///
/// ⚠️ 已知问题：
/// 1. OSS 模式不区分 MainApp 和 UpgradeApp 更新
/// 2. UpgradeApp.exe 必须放在 update/ 子目录中
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class OssStrategy
{
    public static async Task RunAsync()
    {
        var config = new Configinfo
        {
            UpdateUrl = "https://your-storage.com/versions.json",
            AppSecretKey = "your-secret-key",
            AppName = "MyApp.exe",
            MainAppName = "MyApp.exe",
            ClientVersion = "1.0.0.0",
            ProductId = "my-product-001",
            InstallPath = ".",
        };

        await new GeneralUpdateBootstrap()
            .SetConfig(config)
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"OSS 下载: {e.ProgressPercentage}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"OSS 版本 {e.Version} 下载完成"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"OSS 错误: {e.Message}"))
            .LaunchAsync();
    }
}
