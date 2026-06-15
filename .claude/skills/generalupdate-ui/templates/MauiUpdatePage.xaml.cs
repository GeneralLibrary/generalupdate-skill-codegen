using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

namespace MauiUpdate.ViewModels;

/// <summary>
/// [Skill Auto-generated] MAUI Update Page ViewModel
/// Uses GeneralUpdate.Maui.Android or GeneralUpdate.Core
/// </summary>
public partial class UpdateViewModel : ObservableObject
{
    private readonly string _updateUrl;
    private readonly string _secretKey;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private string _versionText = "Checking...";
    [ObservableProperty] private string _releaseNotes = "";
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private string _speedText = "";
    [ObservableProperty] private bool _isUpdating;

    public UpdateViewModel(string updateUrl, string secretKey)
    {
        _updateUrl = updateUrl;
        _secretKey = secretKey;
    }

    [RelayCommand]
    private async Task StartUpdateAsync()
    {
        if (IsUpdating) return;

        IsUpdating = true;
        _cts = new CancellationTokenSource();

        try
        {
            StatusText = "Connecting to server...";

            var bootstrap = new GeneralUpdateBootstrap()
                .SetSource(_updateUrl, _secretKey)
                .SetOption(Option.AppType, AppType.OssClient)
                .AddListenerUpdateInfo((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        VersionText = e.Version ?? "Unknown version";
                    });
                })
                .AddListenerMultiDownloadStatistics((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProgressValue = e.ProgressValue / 100.0;
                        StatusText = $"{e.ProgressValue:F1}%";
                        SpeedText = e.Speed ?? "";
                    });
                })
                .AddListenerMultiDownloadCompleted((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusText = "Download completed, installing...";
                    });
                })
                .AddListenerMultiAllDownloadCompleted((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusText = "Update Completed!";
                    });
                })
                .AddListenerException((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusText = $"Error: {e.Message}";
                    });
                });

            await bootstrap.LaunchAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"Update failed: {ex.Message}";
        }
        finally
        {
            IsUpdating = false;
        }
    }
}
