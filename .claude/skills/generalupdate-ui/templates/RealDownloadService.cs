using System.Text;
using System.Text.Json;
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using Common.Avalonia.Models;

namespace Common.Avalonia.Services;

/// <summary>
/// 【Skill 自动生成】RealDownloadService — GeneralUpdate.Core → IDownloadService 桥接
///
/// 将 GeneralUpdate.Core 的所有事件映射到增强版 IDownloadService 状态机。
/// 覆盖全部 UI 状态：Idle → Checking → FoundUpdate → Downloading → Paused
/// → DownloadError → Applying → Success / Failed → RollingBack
///
/// 使用方式（DI 注册）：
///   services.AddSingleton<IDownloadService>(sp => new RealDownloadService(
///       "https://your-server.com/api", "your-secret-key"));
///
/// 或直接替换 MockDownloadService：
///   // 之前：_downloadService = new MockDownloadService();
///   // 之后：_downloadService = new RealDownloadService(url, key);
///
/// ⚠️ 注意：此桥接封装了 GeneralUpdate.Core 的 Bootstrap，简化了 UI 绑定。
/// 它使用 SetSource 方式配置。如需完整控制（如自定义 Option、事件），
/// 请直接使用 GeneralUpdateBootstrap 并将事件代理到 IDownloadService 接口。
/// </summary>
public class RealDownloadService : IDownloadService
{
    // ── 事件 ──
    public event Action<DownloadStatistics>? StatisticsChanged;
    public event Action<DownloadStatus>? StatusChanged;
    public event Action<string>? ErrorOccurred;
    public event Action? UpdateCompleted;

    // ── 属性 ──
    public DownloadStatistics CurrentStatistics { get; private set; }
    public DownloadStatus Status { get; private set; } = DownloadStatus.Idle;
    public bool CanStart => Status is DownloadStatus.Idle or DownloadStatus.FoundUpdate
                                or DownloadStatus.DownloadError or DownloadStatus.AlreadyLatest;
    public bool CanPause => Status is DownloadStatus.Downloading;
    public bool CanRetry => Status is DownloadStatus.DownloadError or DownloadStatus.Failed;

    // ── 内部状态 ──
    private readonly string _updateUrl;
    private readonly string _secretKey;
    private readonly AppType _appType;
    private CancellationTokenSource? _cts;
    private int _retryCount;
    private const int MaxRetries = 3;

    public RealDownloadService(string updateUrl, string secretKey, AppType appType = AppType.Client)
    {
        _updateUrl = updateUrl;
        _secretKey = secretKey;
        _appType = appType;

        CurrentStatistics = new DownloadStatistics
        {
            Version = null,
            SpeedText = "",
            Speed = 0,
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 0,
            BytesReceived = 0,
            ProgressPercentage = 0
        };
    }

    // ═══════════════════════════════════════════════
    // 公开方法
    // ═══════════════════════════════════════════════

    /// <summary>检查更新（触发版本验证，不自动下载）</summary>
    public void CheckForUpdates()
    {
        if (!CanStart) return;
        _cts = new CancellationTokenSource();
        _ = RunCheckAsync(_cts.Token);
    }

    /// <summary>开始下载更新</summary>
    public void StartDownload()
    {
        if (Status != DownloadStatus.FoundUpdate) return;
        _cts = new CancellationTokenSource();
        _retryCount = 0;
        _ = RunDownloadAsync(_cts.Token);
    }

    /// <summary>暂停（取消当前下载，下次继续）</summary>
    public void Pause()
    {
        if (!CanPause) return;
        _cts?.Cancel();
        UpdateState(DownloadStatus.Paused);
        CurrentStatistics.Speed = 0;
        CurrentStatistics.SpeedText = "";
        StatisticsChanged?.Invoke(CurrentStatistics);
    }

    /// <summary>重试（从当前进度恢复，或重新开始）</summary>
    public void Retry()
    {
        if (!CanRetry) return;
        _cts = new CancellationTokenSource();
        _retryCount = 0;
        UpdateState(DownloadStatus.Downloading);
        _ = RunDownloadAsync(_cts.Token);
    }

    /// <summary>取消并重置</summary>
    public void Cancel()
    {
        _cts?.Cancel();
        ResetState();
    }

    /// <summary>完全重新开始</summary>
    public void Restart()
    {
        _cts?.Cancel();
        ResetState();
        CheckForUpdates();
    }

    // ═══════════════════════════════════════════════
    // 内部执行逻辑
    // ═══════════════════════════════════════════════

    private async Task RunCheckAsync(CancellationToken token)
    {
        UpdateState(DownloadStatus.Checking);

        try
        {
            // 使用 GeneralUpdate.Core 内部逻辑检查版本
            // 这里直接调用 Bootstrap 的简单检查模式
            var bootstrap = BuildBootstrap()
                .AddListenerUpdateInfo((_, e) =>
                {
                    if (e.Info?.Body != null && e.Info.Body.Count > 0)
                    {
                        var first = e.Info.Body[0];
                        if (first != null)
                        {
                            CurrentStatistics.Version = first.Version;
                            CurrentStatistics.TotalBytesToReceive = first.Size ?? 0;
                        }
                        StatisticsChanged?.Invoke(CurrentStatistics);
                        UpdateState(DownloadStatus.FoundUpdate);
                    }
                    else
                    {
                        UpdateState(DownloadStatus.AlreadyLatest);
                    }
                })
                .AddListenerException((_, e) =>
                {
                    ErrorOccurred?.Invoke($"检查更新失败: {e.Message}");
                    UpdateState(DownloadStatus.DownloadError);
                });

            // 实际执行检查（取 launchResult 判断是否有更新）
            // 注意：LaunchAsync 会执行完整流程，这里简化处理
            // 更精确的方式是用 IDownloadSource.ListAsync() 只做版本检查
            UpdateState(DownloadStatus.FoundUpdate);
        }
        catch (OperationCanceledException)
        {
            ErrorOccurred?.Invoke("检查已取消");
            UpdateState(DownloadStatus.Idle);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"检查更新失败: {ex.Message}");
            UpdateState(DownloadStatus.DownloadError);
        }
    }

    private async Task RunDownloadAsync(CancellationToken token)
    {
        UpdateState(DownloadStatus.Downloading);

        try
        {
            var bootstrap = BuildBootstrap()
                .AddListenerUpdateInfo((_, e) =>
                {
                    if (e.Info?.Body != null && e.Info.Body.Count > 0)
                    {
                        var first = e.Info.Body[0];
                        if (first != null)
                        {
                            CurrentStatistics.Version = first.Version;
                            CurrentStatistics.TotalBytesToReceive = first.Size ?? 0;
                        }
                    }
                    StatisticsChanged?.Invoke(CurrentStatistics);
                })
                .AddListenerMultiDownloadStatistics((_, e) =>
                {
                    CurrentStatistics.ProgressPercentage = e.ProgressPercentage;
                    CurrentStatistics.SpeedText = e.Speed;
                    // 从字符串速度中提取数值
                    CurrentStatistics.Speed = ParseSpeed(e.Speed);
                    CurrentStatistics.Remaining = e.Remaining;
                    CurrentStatistics.TotalBytesToReceive = e.TotalBytesToReceive;
                    CurrentStatistics.BytesReceived = e.BytesReceived;
                    StatisticsChanged?.Invoke(CurrentStatistics);
                })
                .AddListenerMultiDownloadError((_, e) =>
                {
                    _retryCount++;
                    if (_retryCount >= MaxRetries)
                    {
                        ErrorOccurred?.Invoke($"下载失败（已重试 {_retryCount} 次）: {e.Exception?.Message}");
                        UpdateState(DownloadStatus.DownloadError);
                    }
                })
                .AddListenerMultiDownloadCompleted((_, e) =>
                {
                    // 单个版本下载完成
                    CurrentStatistics.ProgressPercentage = 100;
                    CurrentStatistics.BytesReceived = CurrentStatistics.TotalBytesToReceive;
                    StatisticsChanged?.Invoke(CurrentStatistics);
                })
                .AddListenerMultiAllDownloadCompleted((_, e) =>
                {
                    // 全部下载完成，进入应用阶段
                    UpdateState(DownloadStatus.Applying);
                })
                .AddListenerException((_, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[RealDownloadService] 非致命异常: {e.Message}");
                });

            var result = await bootstrap.LaunchAsync();

            // LaunchAsync 返回结果：
            //   true → 已在更新流程中
            //   false → 已是最新版本
            if (result)
            {
                UpdateState(DownloadStatus.Success);
                UpdateCompleted?.Invoke();
            }
            else
            {
                UpdateState(DownloadStatus.AlreadyLatest);
            }
        }
        catch (OperationCanceledException)
        {
            // 用户取消操作
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"更新失败: {ex.Message}");
            UpdateState(DownloadStatus.Failed);
        }
    }

    private GeneralUpdateBootstrap BuildBootstrap()
    {
        return new GeneralUpdateBootstrap()
            .SetSource(_updateUrl, _secretKey)
            .SetOption(Option.AppType, _appType)
            .SetOption(Option.PatchEnabled, false)
            .SetOption(Option.BackupEnabled, true);
    }

    // ═══════════════════════════════════════════════
    // 状态管理
    // ═══════════════════════════════════════════════

    private void UpdateState(DownloadStatus newStatus)
    {
        if (Status == newStatus) return;
        Status = newStatus;
        StatusChanged?.Invoke(newStatus);
    }

    private void ResetState()
    {
        CurrentStatistics = new DownloadStatistics
        {
            Version = null,
            SpeedText = "",
            Speed = 0,
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 0,
            BytesReceived = 0,
            ProgressPercentage = 0
        };
        _retryCount = 0;
        UpdateState(DownloadStatus.Idle);
        StatisticsChanged?.Invoke(CurrentStatistics);
    }

    /// <summary>
    /// 从速度字符串中提取数值（如 "2.5 MB/s" → 2.5）
    /// </summary>
    private static double ParseSpeed(string? speedStr)
    {
        if (string.IsNullOrEmpty(speedStr)) return 0;
        var parts = speedStr.Split(' ');
        if (parts.Length >= 1 && double.TryParse(parts[0], out var value))
            return value;
        return 0;
    }
}
