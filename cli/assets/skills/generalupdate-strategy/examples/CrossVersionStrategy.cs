using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Common.Download;

/// <summary>
/// 跨版本直跳更新（CVP — Cross-Version Package）
///
/// 适用于用户长期未更新（如 v1.0 → v3.0），中间版本逐个下载太慢的场景。
///
/// 服务端构建：取两个全量包 ZIP，通过 DiffPipeline 生成差分包。
/// 客户端优先尝试 CVP 包，失败自动退化为链式重试（v5.0+ 特性）。
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class CrossVersionStrategy
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
            {
                if (e.Info?.Body != null)
                    foreach (var v in e.Info.Body)
                        Console.WriteLine($"[CVP] 版本: {v.Version} (跨版本: {v.IsForcibly})");
            })
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[CVP] 下载: {e.ProgressPercentage}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[CVP] 包下载完成: {e.Version}"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[CVP] 错误: {e.Message}"))
            .LaunchAsync();
    }
}
