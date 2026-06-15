using Common.Avalonia.Models;
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

namespace Common.Avalonia.Services;

/// <summary>
/// 【Skill 自动生成】RealDownloadService — 将 GeneralUpdate.Core 事件桥接到 IDownloadService 接口
///
/// 替换 MockDownloadService 的即用型桥接实现。
/// UI 的 ViewModel 不需要任何修改，只需在 DI 容器中将 MockDownloadService 替换为 RealDownloadService。
///
/// 使用方式（在 App.axaml.cs 或 Program.cs 中）：
///
///   // 之前：services.AddSingleton<IDownloadService, MockDownloadService>();
///   // 之后：
///   services.AddSingleton<IDownloadService>(sp => new RealDownloadService(
///       "https://your-server.com/api",
///       "your-secret-key",
///       AppType.Client
///   ));
/// </summary>
public class RealDownloadService : IDownloadService
{
    public event Action<DownloadStatistics>? ProgressChanged;
    public event Action<DownloadStatus>? StatusChanged;
    public event Action? DownloadCompleted;

    public DownloadStatistics CurrentStatistics { get; private set; }
    public DownloadStatus Status { get; private set; } = DownloadStatus.NotStarted;

    private readonly string _updateUrl;
    private readonly string _secretKey;
    private readonly AppType _appType;
    private CancellationTokenSource? _cts;

    public RealDownloadService(string updateUrl, string secretKey, AppType appType = AppType.Client)
    {
        _updateUrl = updateUrl;
        _secretKey = secretKey;
        _appType = appType;

        CurrentStatistics = new DownloadStatistics
        {
            Version = "检测中...",
            Speed = 0,
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 0,
            BytesReceived = 0,
            ProgressPercentage = 0
        };
    }

    public void Start()
    {
        if (Status == DownloadStatus.Downloading)
        {
            Pause();
            return;
        }

        _cts = new CancellationTokenSource();
        UpdateStatus(DownloadStatus.Downloading);
        _ = StartUpdateAsync(_cts.Token);
    }

    public void Pause()
    {
        // GeneralUpdate 当前版本不支持暂停下载
        // 此处实现为取消当前下载
        _cts?.Cancel();
        UpdateStatus(DownloadStatus.Paused);
        CurrentStatistics.Speed = 0;
        ProgressChanged?.Invoke(CurrentStatistics);
    }

    public void Stop()
    {
        _cts?.Cancel();
        UpdateStatus(DownloadStatus.NotStarted);
        CurrentStatistics = new DownloadStatistics
        {
            Version = "已停止",
            Speed = 0,
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 0,
            BytesReceived = 0,
            ProgressPercentage = 0
        };
        ProgressChanged?.Invoke(CurrentStatistics);
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    private async Task StartUpdateAsync(CancellationToken token)
    {
        try
        {
            var bootstrap = new GeneralUpdateBootstrap()
                .SetSource(_updateUrl, _secretKey)
                .SetOption(Option.AppType, _appType)
                .SetOption(Option.PatchEnabled, false)
                .SetOption(Option.BackupEnabled, false)

                .AddListenerUpdateInfo((_, e) =>
                {
                    var stats = CurrentStatistics;
                    stats.Version = e.Version;
                    stats.TotalBytesToReceive = e.Size;
                    ProgressChanged?.Invoke(stats);
                })
                .AddListenerMultiDownloadStatistics((_, e) =>
                {
                    var stats = CurrentStatistics;
                    stats.ProgressPercentage = e.ProgressValue;
                    stats.Speed = ParseSpeed(e.Speed);
                    stats.Remaining = e.Remaining;
                    stats.BytesReceived = (long)(e.ProgressValue / 100.0 * stats.TotalBytesToReceive);
                    ProgressChanged?.Invoke(stats);
                })
                .AddListenerMultiDownloadCompleted((_, e) =>
                {
                    var stats = CurrentStatistics;
                    stats.ProgressPercentage = 100;
                    stats.BytesReceived = stats.TotalBytesToReceive;
                    ProgressChanged?.Invoke(stats);
                })
                .AddListenerMultiAllDownloadCompleted((_, e) =>
                {
                    UpdateStatus(DownloadStatus.Completed);
                    DownloadCompleted?.Invoke();
                })
                .AddListenerException((_, e) =>
                {
                    // 异常通过 EventManager 分发，不中断流程
                });

            var result = await bootstrap.LaunchAsync();

            if (!result)
            {
                // 已是最新版本
                UpdateStatus(DownloadStatus.Completed);
                DownloadCompleted?.Invoke();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RealDownloadService] 更新异常: {ex.Message}");
            Stop();
        }
    }

    private void UpdateStatus(DownloadStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
            StatusChanged?.Invoke(newStatus);
        }
    }

    /// <summary>
    /// 解析 GeneralUpdate 的速度字符串（如 "2.5 MB/s"）为 double（MB/s）
    /// </summary>
    private static double ParseSpeed(string? speedStr)
    {
        if (string.IsNullOrEmpty(speedStr)) return 0;
        var parts = speedStr.Split(' ');
        if (parts.Length >= 2 && double.TryParse(parts[0], out var value))
            return value;
        return 0;
    }
}
