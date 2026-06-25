using System.Text;
using System.Text.Json;
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

// NOTE: 命名空间可根据项目结构调整，例如：
//   WPF:     namespace MyApp.Services;
//   Avalonia: namespace Common.Avalonia.Services;
//   WinForms: namespace MyApp.Services;
//   MAUI:     namespace MyApp.Services;
namespace Common.Avalonia.Services;

/// <summary>
/// 【Skill 自动生成】RealDownloadService — GeneralUpdate.Core → IDownloadService 桥接
///
/// 将 GeneralUpdate.Core 的所有事件映射到增强版 IDownloadService 状态机。
/// 覆盖全部 UI 状态：Idle → Checking → FoundUpdate → Downloading → Paused
/// → DownloadError → Applying → Success / Failed → RollingBack
///
/// ⚠️ 针对 NuGet v10.5.0-rc.1 API。
/// Configinfo → UpdateRequest 替换，命名空间更新。
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
    private CancellationTokenSource? _cts;
    private int _retryCount;
    private const int MaxRetries = 3;

    public RealDownloadService(string updateUrl, string secretKey)
    {
        _updateUrl = updateUrl;
        _secretKey = secretKey;

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

    public void CheckForUpdates()
    {
        if (!CanStart) return;
        _cts = new CancellationTokenSource();
        _ = RunCheckAsync(_cts.Token);
    }

    public void StartDownload()
    {
        if (Status != DownloadStatus.FoundUpdate) return;
        _cts = new CancellationTokenSource();
        _retryCount = 0;
        _ = RunDownloadAsync(_cts.Token);
    }

    public void Pause()
    {
        if (!CanPause) return;
        _cts?.Cancel();
        UpdateState(DownloadStatus.Paused);
        CurrentStatistics.Speed = 0;
        CurrentStatistics.SpeedText = "";
        StatisticsChanged?.Invoke(CurrentStatistics);
    }

    public void Retry()
    {
        if (!CanRetry) return;
        _cts = new CancellationTokenSource();
        _retryCount = 0;
        UpdateState(DownloadStatus.Downloading);
        _ = RunDownloadAsync(_cts.Token);
    }

    public void Cancel()
    {
        _cts?.Cancel();
        ResetState();
    }

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
            var config = new UpdateRequest
            {
                UpdateUrl = _updateUrl,
                AppSecretKey = _secretKey,
                ClientVersion = GetCurrentVersion(),
                ProductId = "check",
                InstallPath = ".",
            };

            // 启动 Bootstrap 进行真实版本验证
            await new GeneralUpdateBootstrap()
                .SetConfig(config)
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
                })
                .LaunchAsync();
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
            var config = new UpdateRequest
            {
                UpdateUrl = _updateUrl,
                AppSecretKey = _secretKey,
                MainAppName = "MyApp.exe",
                ClientVersion = GetCurrentVersion(),
                ProductId = "my-product-001",
                InstallPath = ".",
            };

            await new GeneralUpdateBootstrap()
                .SetConfig(config)
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
                    CurrentStatistics.ProgressPercentage = 100;
                    CurrentStatistics.BytesReceived = CurrentStatistics.TotalBytesToReceive;
                    StatisticsChanged?.Invoke(CurrentStatistics);
                })
                .AddListenerMultiAllDownloadCompleted((_, e) =>
                {
                    UpdateState(DownloadStatus.Applying);
                })
                .AddListenerException((_, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[RealDownloadService] 非致命异常: {e.Message}");
                })
                .LaunchAsync();

            // LaunchAsync 返回后表示更新流程已启动
            // 有更新时进程由 Upgrade 接管
            UpdateState(DownloadStatus.Success);
            UpdateCompleted?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // 用户取消
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"更新失败: {ex.Message}");
            UpdateState(DownloadStatus.Failed);
        }
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

    private static string GetCurrentVersion()
    {
        return System.Reflection.Assembly.GetEntryAssembly()
            ?.GetName()?.Version?.ToString(4) ?? "1.0.0.0";
    }

    private static double ParseSpeed(string? speedStr)
    {
        if (string.IsNullOrEmpty(speedStr)) return 0;
        var parts = speedStr.Split(' ');
        if (parts.Length >= 1 && double.TryParse(parts[0], out var value))
            return value;
        return 0;
    }
}
