using CommunityToolkit.Mvvm.ComponentModel;
using Common.Avalonia.Models;

namespace Upgrade.WPF.ViewModels;

/// <summary>
/// [Skill Auto-generated] WPF + WPFDevelopers-specific ViewModel
/// Contains SpeedText property required by circular progress bar
/// </summary>
public partial class WpfDevelopersUpdateViewModel : ObservableObject
{
    private readonly IDownloadService _downloadService;

    [ObservableProperty] private DownloadStatistics _statistics;
    [ObservableProperty] private DownloadStatus _status;
    [ObservableProperty] private string _statusText = "Ready";
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
                DownloadStatus.Downloading => "Downloading...",
                DownloadStatus.Completed => "Update Completed!",
                DownloadStatus.Paused => "Paused",
                _ => ""
            };
        });
    }

    private void OnCompleted() { }

    public void StartDownload() => _downloadService.Start();
}
