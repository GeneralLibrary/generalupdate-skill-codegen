using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// SignalR 推送更新策略
///
/// 适用于需要服务端主动控制更新时机的场景。
/// 客户端连接 SignalR Hub，服务端推送更新通知后触发更新。
///
/// NuGet:
///   dotnet add package GeneralUpdate.Core
///   dotnet add package Microsoft.AspNetCore.SignalR.Client
/// </summary>
public static class PushStrategy
{
    public static async Task RunAsync()
    {
        var updateUrl = "https://your-server.com/api";
        var secretKey = "your-secret-key";
        var hubUrl = "https://your-server.com/hub/upgrade";

        var config = new UpdateRequest
        {
            UpdateUrl = updateUrl,
            AppSecretKey = secretKey,
            MainAppName = "MyApp.exe",
            ClientVersion = "1.0.0.0",
            ProductId = "my-product-001",
            InstallPath = ".",
        };

        // 连接到 SignalR Hub
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("OnPushUpgrade", async (message) =>
        {
            Console.WriteLine($"[推送] 收到更新通知: {message}");
            await StartUpdateAsync(config);
        });

        connection.On<string>("OnForceUpgrade", async (message) =>
        {
            Console.WriteLine($"[推送] 收到强制更新通知: {message}");
            await StartUpdateAsync(config);
        });

        try
        {
            await connection.StartAsync();
            Console.WriteLine("[推送] 已连接到更新推送服务");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[推送] 连接失败: {ex.Message}");
        }

        // 保持应用运行
        await Task.Delay(Timeout.Infinite);
    }

    private static async Task StartUpdateAsync(UpdateRequest config)
    {
        try
        {
            await new GeneralUpdateBootstrap()
                .SetConfig(config)
                .AddListenerMultiDownloadStatistics((_, e) =>
                    Console.WriteLine($"[推送更新] 下载: {e.ProgressPercentage}%"))
                .AddListenerMultiDownloadCompleted((_, e) =>
                    Console.WriteLine($"[推送更新] 下载完成: {e.Version}"))
                .AddListenerException((_, e) =>
                    Console.WriteLine($"[推送更新] 错误: {e.Message}"))
                .LaunchAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[推送更新] 启动失败: {ex.Message}");
        }
    }
}
