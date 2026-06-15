using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// 策略 5：跨版本直跳更新（CVP — Cross-Version Package）
///
/// 适用场景：
/// - 用户长期没有更新（如 v1.0 → 最新 v3.0）
/// - 中间有大量版本，逐个下载太慢
/// - 需要从指定版本直接跳到目标版本
///
/// 工作原理：
/// 服务端自动构建：
///   1. 取两个版本的全量包归档（如 v1.0 和 v3.0 的 ZIP）
///   2. CrossVersionPacketBuilder 调用 DiffPipeline.CleanAsync() 生成差分包
///   3. 上传差分包到对象存储，写入 TbPacket(IsCrossVersion=true)
///
/// 客户端流程：
///   Verify → 服务端返回 CVP 包（一个 ZIP）
///   → 下载 → 应用（一次 patch 操作）→ 完成
///
/// CVP + 链式兜底（从 v5.0 开始支持，Issue #499）：
///   服务器一次返回所有路径（CVP 包 + 链式包），客户端优先走 CVP，
///   失败自动退化为链式重试，无需二次请求。
///
/// NuGet: dotnet add package GeneralUpdate.Core
///
/// ⚠️ 注意事项：
/// - 需要 GeneralSpacestation 服务端支持 CrossVersion 构建
/// - CVP 构建是异步的（Redis 队列 + BackgroundService 消费）
/// - 构建材料是全量包归档（TbVersionArchive）
/// </summary>
public static class CrossVersionStrategy
{
    public static async Task RunAsync()
    {
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-server.com/api",
                "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)
            .SetOption(Option.BackupEnabled, true)       // CVP 建议启用备份

            // 跨版本更新配置
            // 服务端会自动判断返回 CVP 包还是链式包
            // 客户端优先尝试 CVP，失败自动退链式

            .AddListenerUpdateInfo((_, e) =>
            {
                Console.WriteLine($"[CVP] 版本: {e.Version} | " +
                    $"跨版本: {e.IsCrossVersion} | " +
                    $"文件数: {e.FileCount} | 大小: {e.Size}");
            })
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[CVP] 下载: {e.ProgressValue}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
            {
                var lastVer = e.Versions?.LastOrDefault();
                Console.WriteLine($"[CVP] 包下载完成: {lastVer?.Version} " +
                    $"(跨版本: {lastVer?.IsCrossVersion})");
            })
            .AddListenerException((_, e) =>
            {
                Console.WriteLine($"[CVP] 错误: {e.Message}");
                // 如果 CVP 失败，GeneralUpdate 会自动退化为链式重试
            });

        await bootstrap.LaunchAsync();
    }
}
