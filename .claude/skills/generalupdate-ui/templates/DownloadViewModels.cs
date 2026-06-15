using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Common.Avalonia.Models;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// 【Skill 自动生成】增强版下载 ViewModel — 覆盖全部 UI 状态
///
/// 适用于所有 UI 框架（Avalonia/WPF/WinForms/MAUI）。
/// 包含完整的 MVVM 命令绑定和状态转换逻辑。
/// </summary>
public partial class EnhancedDownloadViewModel : ObservableObject
{
    private readonly IDownloadService _downloadService;

    // ── 可观察属性 ──

    [ObservableProperty]
    private DownloadStatistics _statistics;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private DownloadStatus _status;

    [ObservableProperty]
    private string _statusText = "准备就绪";

    [ObservableProperty]
    private string _versionText = "";

    [ObservableProperty]
    private string _speedText = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isIndeterminate;

    [ObservableProperty]
    private bool _isErrorVisible;

    [ObservableProperty]
    private bool _isProgressVisible;

    [ObservableProperty]
    private bool _isUpdateFound;

    [ObservableProperty]
    private bool _isCompleted;

    private string _lastError = "";

    public EnhancedDownloadViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;

        _downloadService.StatisticsChanged += OnStatisticsChanged;
        _downloadService.StatusChanged += OnStatusChanged;
        _downloadService.ErrorOccurred += OnError;
        _downloadService.UpdateCompleted += OnCompleted;

        Statistics = _downloadService.CurrentStatistics;
        Status = _downloadService.Status;
        UpdateVisibility();
    }

    // ═══════════════════════════════════════════════
    // 命令
    // ═══════════════════════════════════════════════

    private bool CanStart => _downloadService.CanStart;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Check() => _downloadService.CheckForUpdates();

    private bool CanDownload => Status == DownloadStatus.FoundUpdate;
    [RelayCommand(CanExecute = nameof(CanDownload))]
    private void Download() => _downloadService.StartDownload();

    private bool CanPauseDownload => _downloadService.CanPause;
    [RelayCommand(CanExecute = nameof(CanPauseDownload))]
    private void Pause() => _downloadService.Pause();

    private bool CanRetryDownload => _downloadService.CanRetry;
    [RelayCommand(CanExecute = nameof(CanRetryDownload))]
    private void Retry() => _downloadService.Retry();

    private bool CanCancelOp => Status is DownloadStatus.Downloading
                                or DownloadStatus.Paused
                                or DownloadStatus.DownloadError;
    [RelayCommand(CanExecute = nameof(CanCancelOp))]
    private void Cancel() => _downloadService.Cancel();

    // ═══════════════════════════════════════════════
    // 事件处理
    // ═══════════════════════════════════════════════

    private void OnStatisticsChanged(DownloadStatistics stats)
    {
        Dispatch(() =>
        {
            Statistics = stats;
            if (stats.Speed > 0)
                SpeedText = $"{stats.Speed:F1} MB/s";
        });
    }

    private void OnStatusChanged(DownloadStatus status)
    {
        Dispatch(() =>
        {
            Status = status;
            UpdateVisibility();
            UpdateStatusText();
        });
    }

    private void OnError(string error)
    {
        Dispatch(() =>
        {
            _lastError = error;
            ErrorMessage = error;
            IsErrorVisible = true;
        });
    }

    private void OnCompleted()
    {
        Dispatch(() =>
        {
            IsCompleted = true;
            StatusText = "更新完成！应用即将重启";
        });
    }

    // ═══════════════════════════════════════════════
    // UI 状态同步
    // ═══════════════════════════════════════════════

    private void UpdateVisibility()
    {
        IsProgressVisible = Status is DownloadStatus.Downloading
                            or DownloadStatus.Applying
                            or DownloadStatus.UpgradeProgress;

        IsIndeterminate = Status is DownloadStatus.Checking
                          or DownloadStatus.Applying
                          or DownloadStatus.UpgradeProgress
                          or DownloadStatus.RollingBack;

        IsUpdateFound = Status == DownloadStatus.FoundUpdate;
        IsErrorVisible = Status is DownloadStatus.DownloadError
                         or DownloadStatus.Failed;
        IsCompleted = Status == DownloadStatus.Success;
    }

    private void UpdateStatusText()
    {
        StatusText = Status switch
        {
            DownloadStatus.Idle => "准备就绪",
            DownloadStatus.Checking => "正在检查更新...",
            DownloadStatus.FoundUpdate => "发现新版本！",
            DownloadStatus.AlreadyLatest => "已是最新版本 ✓",
            DownloadStatus.Downloading => $"正在下载 ({Statistics.ProgressPercentage:F0}%)",
            DownloadStatus.Paused => $"已暂停 ({Statistics.ProgressPercentage:F0}%)",
            DownloadStatus.DownloadError => "下载出错",
            DownloadStatus.Applying => "正在安装更新...",
            DownloadStatus.UpgradeProgress => "正在完成更新...",
            DownloadStatus.Success => "更新完成！",
            DownloadStatus.Failed => $"更新失败: {_lastError}",
            DownloadStatus.RollingBack => "正在回滚到上一个版本...",
            _ => ""
        };
    }

    /// <summary>
    /// 将操作调度到 UI 线程（框架适配）
    /// 在 WPF 中使用 Application.Current.Dispatcher
    /// 在 Avalonia 中使用 Avalonia.Threading.Dispatcher.UIThread
    /// 在 WinForms 中使用 Control.BeginInvoke
    /// 在 MAUI 中使用 MainThread.BeginInvokeOnMainThread
    /// </summary>
    private static void Dispatch(Action action)
    {
        // 框架自动适配 — 在对应的 UI 项目中替换为正确的 Dispatcher
        action();
    }
}
