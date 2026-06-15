using CommunityToolkit.Mvvm.ComponentModel;
using Common.Avalonia.Models;

namespace Upgrade.WPF.ViewModels;

/// <summary>
/// 【Skill 自动生成】WPF + WPFDevelopers 专用 ViewModel
/// 包含圆形进度条所需的 SpeedText 属性
/// </summary>
public partial class WpfDevelopersUpdateViewModel : ObservableObject
{
    private readonly IDownloadService _downloadService;

    [ObservableProperty] private DownloadStatistics _statistics;
    [ObservableProperty] private DownloadStatus _status;
    [ObservableProperty] private string _statusText = "准备就绪";
    [ObservableProperty] private string _speedText = "";

    public WpfDevelopersUpdateViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        _downloadService.ProgressChanged += OnProgressChanged;
        _downloadService.StatusChanged += OnStatusChanged;
        _downloadService.DownloadCompleted += OnCompleted;

        Statistics = _downloadService.CurrentStatistics;
        Status = _downloadService.Status;
    }

    private void OnProgressChanged(DownloadStatistics stats)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Statistics = stats;
            SpeedText = $"{stats.Speed:F1} MB/s";
        });
    }

    private void OnStatusChanged(DownloadStatus status)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Status = status;
            StatusText = status switch
            {
                DownloadStatus.Downloading => "正在下载...",
                DownloadStatus.Completed => "更新完成！",
                DownloadStatus.Paused => "已暂停",
                _ => ""
            };
        });
    }

    private void OnCompleted() { }

    public void StartDownload() => _downloadService.Start();
}
