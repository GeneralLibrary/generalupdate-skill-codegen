using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// 策略 3：静默后台更新
///
/// 适用场景：
/// - 用户长期不关闭应用（如桌面工具、监控面板）
/// - 希望后台下载更新，不打扰用户
/// - 用户下次打开应用时自动切换到新版本
///
/// 工作流程：
/// 1. 应用启动 → SilentPollOrchestrator 开始后台轮询
/// 2. 轮询到新版本 → 后台下载所有包（不启动 Upgrade 进程）
/// 3. 下载完成 → 写入 IPC 文件，标记准备就绪
/// 4. 用户关闭应用 → ProcessExit → 触发 Upgrade 进程 → 更新 → 下次启动是新版本
///
/// NuGet: dotnet add package GeneralUpdate.Core
///
/// ⚠️ 已知问题（来自 Issue #484、#471、#443）：
/// 1. ProcessExit 事件不保证触发（FailFast、TerminateProcess、Ctrl+C 时不会）
///    解决方案：在应用关闭逻辑中显式调用 TryLaunchUpgrade()
/// 2. 静默模式 PatchMiddleware 可能抛异常（PatchEnabled 的默认行为）
///    解决方案：显式设置 SetOption(Option.PatchEnabled, false)
/// 3. manifest.json 的默认非空字段会阻塞自动版本发现
///    解决方案：确保 manifest.json 中字段为空或正确
/// 4. 静默模式默认更新完不启动应用（#443）
///    解决方案：通过 SetOption(Option.SilentAutoRestart, true) 配置
/// </summary>
public class SilentStrategy : IDisposable
{
    private GeneralUpdateBootstrap? _bootstrap;
    private SilentPollOrchestrator? _orchestrator;

    public async Task RunAsync()
    {
        _bootstrap = new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-server.com/api",
                "your-secret-key")
            .SetOption(Option.AppType, AppType.Client)

            // 静默模式配置
            .SetOption(Option.Silent, true)                  // 启用静默模式
            .SetOption(Option.SilentPollIntervalMinutes, 60) // 每60分钟检查一次
            .SetOption(Option.PatchEnabled, false)           // 关闭差分（避免#471 Bug）
            .SetOption(Option.BackupEnabled, true)           // 静默模式建议启用备份

            .AddListenerUpdateInfo((_, e) =>
                Console.WriteLine($"[静默] 发现新版本: {e.Version}"))
            .AddListenerMultiDownloadStatistics((_, e) =>
                Console.WriteLine($"[静默] 后台下载: {e.ProgressValue}%"))
            .AddListenerMultiDownloadCompleted((_, e) =>
                Console.WriteLine($"[静默] 版本 {e.Versions?.LastOrDefault()?.Version} 就绪"))
            .AddListenerException((_, e) =>
                Console.WriteLine($"[静默] 错误: {e.Message}"));

        var result = await _bootstrap.LaunchAsync();

        // 获取静默轮询器，以便手动触发更新
        _orchestrator = _bootstrap.SilentOrchestrator;

        if (_orchestrator != null)
        {
            _orchestrator.HasPreparedUpdate += () =>
            {
                Console.WriteLine("[静默] 更新已就绪，将在进程退出时安装");
            };
        }

        // 启动应用主循环...
        await RunApplicationAsync();
    }

    /// <summary>
    /// 应用关闭时主动触发更新，弥补 ProcessExit 不稳定的问题
    /// </summary>
    public void OnAppClosing()
    {
        // 显式触发升级（比依赖 ProcessExit 更可靠）
        try
        {
            _orchestrator?.TryLaunchUpgrade();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[静默] 触发升级失败: {ex.Message}");
        }
    }

    private async Task RunApplicationAsync()
    {
        // 模拟应用主循环
        Console.WriteLine("[静默] 应用正在运行，更新将在后台静默下载...");
        await Task.Delay(TimeSpan.FromHours(8)); // 模拟长时间运行
    }

    public void Dispose()
    {
        OnAppClosing();
    }
}
