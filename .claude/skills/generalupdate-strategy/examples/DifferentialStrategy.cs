using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Differential;
using GeneralUpdate.Core.Enum;

/// <summary>
/// 策略 4：差分增量更新
///
/// 适用场景：
/// - 应用体积大（>100MB）
/// - 用户网络带宽受限
/// - 每次更新只改少量文件
///
/// 工作原理：
/// 服务端：DiffPipeline.CleanAsync(srcDir, tgtDir, patchDir)
///   → 对比新旧版本文件 → 生成 .patch 补丁文件 + 新增文件 + 删除清单
///
/// 客户端：下载差分包 → PipelineBuilder
///   → HashMiddleware (完整性校验)
///   → CompressMiddleware (解压)
///   → PatchMiddleware (应用补丁，DiffPipeline.DirtyAsync)
///
/// NuGet:
///   dotnet add package GeneralUpdate.Core
///   dotnet add package GeneralUpdate.Differential
///
/// ⚠️ 已知问题（来自 Issue #II75WI、#II77NS）：
/// 1. 同名文件在不同目录时，封包可能出错（DefaultCleanMatcher 未用相对路径匹配）
///    解决方案：设置自定义 CleanMatcher
/// 2. 旧 patch 临时文件残留可能导致后续更新失败
///    解决方案：每次更新前清理 TempPath
/// 3. 建议对 patch 增加旧文件的 hash 判断
///    若旧文件 hash 不匹配，跳过该文件并给出错误回调
/// </summary>
public static class DifferentialStrategy
{
    public static async Task RunAsync()
    {
        // 1. 配置 DiffPipeline（差分引擎）
        // 服务端使用 Clean 模式生成差分包时用此配置
        // 客户端使用 Dirty 模式应用差分包时自动注入
        var diffPipeline = new DiffPipelineBuilder()
            .UseDiffer(new StreamingHdiffDiffer())    // 差分算法：HDiffPatch（默认）
            // .UseDiffer(new BsdiffDiffer())          // 备选：BSDIFF
            .UseCleanMatcher(new DefaultCleanMatcher())
            .UseDirtyMatcher(new DefaultDirtyMatcher())
            .WithParallelism(2)                       // 并行度（默认2）
            .Build();

        // 2. 配置 Bootstrap（启用差分）
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-server.com/api",
                "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)
            .SetOption(Option.PatchEnabled, true)       // 启用差分模式
            .SetOption(Option.MaxConcurrency, 3)
            .SetOption(Option.BackupEnabled, true)       // 差分更新建议启用备份
            .SetOption(Option.Format, CompressionFormat.Zip)

            // ⚠️ 每次更新前自动清理临时目录，避免残留文件
            .SetOption(Option.AutoCleanTemp, true)

            .AddListenerMultiDownloadStatistics((_, e) =>
            {
                Console.WriteLine($"[差分] 下载: {e.ProgressValue}% | " +
                    $"速度: {e.Speed}");
            })
            .AddListenerProgress((_, e) =>
            {
                // 包含解压、补丁应用等阶段的详细进度
                Console.WriteLine($"[处理] {e.FileName} — {e.ProgressValue}% ({e.Type})");
            })
            .AddListenerException((_, e) =>
            {
                Console.WriteLine($"[差分] 错误: {e.Message}");
            });

        await bootstrap.LaunchAsync();
    }
}
