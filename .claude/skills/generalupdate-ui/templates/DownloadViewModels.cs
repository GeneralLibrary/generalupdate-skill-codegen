using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Common.Avalonia.Models;
using Common.Avalonia.Services;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// 【Skill 自动生成】通用下载视图模型 — 基于 IDownloadService 的 MVVM ViewModel
///
/// 适用于：Avalonia + SemiUrsa / WPF + LayUI / WPF + WPFDevelopers
/// 只需注入 IDownloadService 的真实实现（RealDownloadService）即可驱动更新UI。
///
/// 使用方式（DI 注册）：
///   services.AddSingleton<IDownloadService, RealDownloadService>();
///   services.AddTransient<MainViewViewModel>();
/// </summary>
public partial class DownloadViewModel : ObservableObject
{
    private readonly IDownloadService _downloadService;

    [ObservableProperty]
    private DownloadStatistics _statistics;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private DownloadStatus _status;

    [ObservableProperty]
    private string _statusText = "准备就绪";

    [ObservableProperty]
    private string _versionText = "";

    public DownloadViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;

        _downloadService.ProgressChanged += OnProgressChanged;
        _downloadService.StatusChanged += OnStatusChanged;
        _downloadService.DownloadCompleted += OnDownloadCompleted;

        Statistics = _downloadService.CurrentStatistics;
        Status = _downloadService.Status;
    }

    private bool CanStart => Status is DownloadStatus.NotStarted
                             or DownloadStatus.Paused;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start() => _downloadService.Start();

    private bool CanStop => Status is DownloadStatus.Downloading
                            or DownloadStatus.Paused;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _downloadService.Stop();

    [RelayCommand]
    private void Restart() => _downloadService.Restart();

    private void OnProgressChanged(DownloadStatistics stats)
    {
        // 在 UI 线程上更新
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Statistics = stats;
            StatusText = stats.ProgressPercentage switch
            {
                < 100 => $"下载中... {stats.ProgressPercentage:F1}%",
                _ => "下载完成，正在处理..."
            };
            VersionText = $"版本: {stats.Version}";
        });
    }

    private void OnStatusChanged(DownloadStatus status)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Status = status;
            StatusText = status switch
            {
                DownloadStatus.NotStarted => "准备就绪",
                DownloadStatus.Downloading => "正在下载更新...",
                DownloadStatus.Paused => "已暂停",
                DownloadStatus.Completed => "更新完成！",
                _ => ""
            };
        });
    }

    private void OnDownloadCompleted()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            StatusText = "更新完成！即将启动...";
        });
    }
}
