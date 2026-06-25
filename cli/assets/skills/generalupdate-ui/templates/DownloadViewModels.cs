using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

            // 版本信息同步
            if (stats.Version != null)
                VersionText = $"版本: {stats.Version}";

            // 速度信息：小于 0.01 MB/s 视为 0
            if (stats.Speed > 0.01)
                SpeedText = $"{stats.Speed:F1} MB/s";
            else
                SpeedText = "";
        });
    }

    private void OnStatusChanged(DownloadStatus status)
    {
        Dispatch(() =>
        {
            Status = status;
            UpdateVisibility();
            UpdateStatusText();

            // 非下载状态清空速度
            if (status is not (DownloadStatus.Downloading or DownloadStatus.Applying))
                SpeedText = "";
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
    /// 将操作调度到 UI 线程（框架适配）。
    /// 通过静态属性注入平台特定的 Dispatcher：
    ///   - WPF:        UIDispatcher = action => Application.Current.Dispatcher.Invoke(action)
    ///   - Avalonia:    UIDispatcher = action => Avalonia.Threading.Dispatcher.UIThread.Post(action)
    ///   - WinForms:    UIDispatcher = action => { var ctx = SynchronizationContext.Current; ctx?.Post(_ => action(), null); }
    ///   - MAUI:        UIDispatcher = action => MainThread.BeginInvokeOnMainThread(action)
    ///   - Console:     UIDispatcher = action => action()  （无需调度）
    ///
    /// 在应用启动时设置一次：
    ///   EnhancedDownloadViewModel.UIDispatcher = action => Application.Current.Dispatcher.Invoke(action);
    /// </summary>
    public static Action<Action> UIDispatcher { get; set; } = action =>
    {
        // 默认：尝试使用 SynchronizationContext（WinForms 兼容）
        var ctx = SynchronizationContext.Current;
        if (ctx != null)
            ctx.Post(_ => action(), null);
        else
            action(); // 控制台/测试环境直接执行
    };

    private static void Dispatch(Action action)
    {
        UIDispatcher(action);
    }
}
