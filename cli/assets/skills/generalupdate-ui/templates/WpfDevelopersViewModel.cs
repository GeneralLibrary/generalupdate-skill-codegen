using CommunityToolkit.Mvvm.ComponentModel;

namespace Upgrade.WPF.ViewModels;

/// <summary>
/// 【Skill 自动生成】WPF + WPFDevelopers 专用 ViewModel
/// 包含圆形进度条所需的 SpeedText 属性。
/// 事件通过 Inject 方法注入，解耦具体的 IDownloadService 实现。
/// </summary>
public partial class WpfDevelopersUpdateViewModel : ObservableObject
{
    private Action<IDownloadService>? _onBind;

    [ObservableProperty] private DownloadStatistics _statistics;
    [ObservableProperty] private DownloadStatus _status;
    [ObservableProperty] private string _statusText = "就绪";
    [ObservableProperty] private string _speedText = "";

    public WpfDevelopersUpdateViewModel()
    {
        Statistics = new DownloadStatistics
        {
            Version = null, SpeedText = "", Speed = 0,
            Remaining = TimeSpan.Zero, TotalBytesToReceive = 0,
            BytesReceived = 0, ProgressPercentage = 0
        };
        Status = DownloadStatus.Idle;
    }

    /// <summary>
    /// 绑定 IDownloadService 事件到 ViewModel。
    /// 在创建 ViewModel 后调用。
    /// </summary>
    public void Bind(IDownloadService downloadService)
    {
        _onBind = ds =>
        {
            ds.StatisticsChanged += OnStatisticsChanged;
            ds.StatusChanged += OnStatusChanged;
            ds.UpdateCompleted += OnCompleted;
        };
        _onBind(downloadService);
    }

    private void OnStatisticsChanged(DownloadStatistics stats)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Statistics = stats;
            SpeedText = stats.Speed > 0.01 ? $"{stats.Speed:F1} MB/s" : "";
        });
    }

    private void OnStatusChanged(DownloadStatus status)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Status = status;
            StatusText = status switch
            {
                DownloadStatus.Idle => "就绪",
                DownloadStatus.Checking => "正在检查更新...",
                DownloadStatus.FoundUpdate => "发现新版本！",
                DownloadStatus.Downloading => "正在下载...",
                DownloadStatus.Paused => "已暂停",
                DownloadStatus.DownloadError => "下载出错",
                DownloadStatus.Applying => "正在安装...",
                DownloadStatus.Success => "更新完成！",
                DownloadStatus.Failed => "更新失败",
                _ => ""
            };
        });
    }

    private void OnCompleted()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusText = "更新完成！应用即将重启";
        });
    }
}
