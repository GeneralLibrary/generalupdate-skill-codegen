using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Common.Download;

/// <summary>
/// 差分增量更新策略
///
/// 适用于应用体积大（>100MB）或带宽受限的场景。
/// 仅传输变动的文件部分，节省 60-90% 带宽。
///
/// 工作原理：
/// 服务端调用 DiffPipeline 生成 .patch 文件
/// 客户端下载后自动应用补丁
///
/// NuGet:
///   dotnet add package GeneralUpdate.Core
///
/// ⚠️ 注意：GeneralUpdate.Core v10.4.6 稳定版已内置差分支持，
/// 无需额外安装 GeneralUpdate.Differential 包。
///
/// ⚠️ 已知问题：
/// 1. 同名文件在不同目录可能封包出错
/// 2. 旧 patch 临时文件残留可能导致后续更新失败
/// </summary>
public static class DifferentialStrategy
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

            // 服务端配置差分时自动启用
        };

        await new GeneralUpdateBootstrap()
            .SetConfig(config)
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[差分] 下载: {e.ProgressPercentage}% | {e.Speed}"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[差分] 版本 {e.Version} 处理完成"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[差分] 错误: {e.Message}"))
            .LaunchAsync();
    }
}
