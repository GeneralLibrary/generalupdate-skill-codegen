using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// Upgrade 升级程序 — 由 Client 进程通过 IPC 启动
///
/// 职责：读取 IPC 数据 → 应用更新 → 启动主程序 → 退出
/// 注意：Upgrade 程序不访问网络（所有数据由 Client 预下载）
///       Upgrade 程序和 Client 必须使用相同的 AppSecretKey
/// </summary>
Console.WriteLine("[Upgrade] 升级程序启动");

try
{
    var result = await new GeneralUpdateBootstrap()
        // Upgrade 模式会从 IPC 文件读取配置，无需 SetSource
        .SetOption(Option.AppType, AppType.Upgrade)

        // 事件监听
        .AddListenerProgress((_, e) =>
            Console.WriteLine($"[Upgrade] 处理: {e.FileName} — {e.ProgressValue}% ({e.Type})"))
        .AddListenerException((_, e) =>
            Console.WriteLine($"[Upgrade] 错误: {e.Message}"))

        .LaunchAsync();

    Console.WriteLine(result
        ? "[Upgrade] 更新完成，主程序已启动"
        : "[Upgrade] 无需更新，主程序已启动");
}
catch (Exception ex)
{
    Console.WriteLine($"[Upgrade] 严重错误: {ex}");
    // Upgrade 失败不应完全静默，记录日志后退出
    // Client 下次启动时会重试
    Environment.ExitCode = 1;
}
