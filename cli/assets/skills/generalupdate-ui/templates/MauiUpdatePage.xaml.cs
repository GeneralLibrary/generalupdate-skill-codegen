using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;

namespace MauiUpdate.ViewModels;

/// <summary>
/// 【Skill 自动生成】MAUI 更新页面 ViewModel
/// 针对 NuGet v10.4.6 稳定版 API
/// </summary>
public partial class UpdateViewModel : ObservableObject
{
    private readonly string _updateUrl;
    private readonly string _secretKey;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private string _versionText = "检测中...";
    [ObservableProperty] private string _releaseNotes = "";
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText = "准备就绪";
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
            StatusText = "正在连接服务器...";

            var config = new Configinfo
            {
                UpdateUrl = _updateUrl,
                AppSecretKey = _secretKey,
                AppName = "MyApp.exe",
                MainAppName = "MyApp.exe",
                ClientVersion = "1.0.0.0",
                ProductId = "my-product-001",
                InstallPath = ".",
            };

            // v10.4.6 稳定版 API：Configinfo + SetConfig + LaunchAsync
            await new GeneralUpdateBootstrap()
                .SetConfig(config)
                .AddListenerUpdateInfo((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        VersionText = e.Info?.Body?.Count > 0 ? e.Info.Body[0]?.Version ?? "未知版本" : "未知版本";
                    });
                })
                .AddListenerMultiDownloadStatistics((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProgressValue = e.ProgressPercentage / 100.0;
                        StatusText = $"{e.ProgressPercentage:F1}%";
                        SpeedText = e.Speed ?? "";
                    });
                })
                .AddListenerMultiDownloadCompleted((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusText = "下载完成，正在安装...";
                    });
                })
                .AddListenerMultiAllDownloadCompleted((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusText = "更新完成！";
                    });
                })
                .AddListenerException((_, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusText = $"错误: {e.Message}";
                    });
                })
                .LaunchAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"更新失败: {ex.Message}";
        }
        finally
        {
            IsUpdating = false;
        }
    }
}
