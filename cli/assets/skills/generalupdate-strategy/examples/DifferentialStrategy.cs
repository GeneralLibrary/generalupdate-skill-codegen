using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

/// <summary>
/// 差分增量更新策略
///
/// 适用于应用体积大（>100MB）或带宽受限的场景。
/// 仅传输变动的文件部分，节省 60-90% 带宽。
///
/// v10.5.0-beta.4 通过 UseDiffPipeline(Action{DiffPipelineBuilder}) 配置差分。
///
/// NuGet:
///   dotnet add package GeneralUpdate.Core
///
/// ⚠️ 注意：GeneralUpdate.Core v10.5.0-beta.4 已内置差分支持，
/// 无需额外安装 GeneralUpdate.Differential 包。
/// </summary>
public static class DifferentialStrategy
{
    public static async Task RunAsync()
    {
        await new GeneralUpdateBootstrap()
            .SetSource(
                updateUrl: "https://your-server.com/api",
                appSecretKey: "your-secret-key")
            .UseDiffPipeline(pipeline =>
            {
                pipeline
                    .WithParallelism(2)
                    .WithStopOnFirstError(true);
            })
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[差分] 下载: {e.ProgressPercentage}% | {e.Speed}"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[差分] 版本 {e.Version} 处理完成"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[差分] 错误: {e.Message}"))
            .LaunchAsync();
    }
}
