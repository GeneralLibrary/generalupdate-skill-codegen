using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// 策略 6：SignalR 推送更新
///
/// 适用场景：
/// - 需要服务端主动控制更新时机
/// - 管理员从后台管理界面点击"推送更新"
/// - 需要立即通知所有在线的客户端
///
/// 工作流程：
/// 1. 客户端连接 SignalR Hub
/// 2. 管理员在后台上传新版本包
/// 3. 管理员点击"推送" → SignalR Hub 通知所有客户端
/// 4. 客户端收到推送 → 触发 GeneralUpdateBootstrap 开始更新
///
/// NuGet:
///   dotnet add package GeneralUpdate.Core
///   dotnet add package Microsoft.AspNetCore.SignalR.Client
///
/// ⚠️ 已知问题（来自 Issue #402）：
/// 1. SignalR Client 支持 Native AOT（需 JSON 协议 + JsonSerializerContext）
/// 2. UpgradeHubService 在 Dispose 后调用 StartAsync 会崩溃
///    解决方案：使用下面的 SafeHubConnection 包装类
/// </summary>
public static class PushStrategy
{
    public static async Task RunAsync()
    {
        var updateUrl = "https://your-server.com/api";
        var secretKey = "your-secret-key";
        var hubUrl = "https://your-server.com/hub/upgrade";

        // 1. 连接到 SignalR Hub
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()   // 自动重连
            .Build();

        // 2. 注册推送事件处理
        connection.On<string>("OnPushUpgrade", async (message) =>
        {
            Console.WriteLine($"[推送] 收到更新通知: {message}");
            await StartUpdateAsync(updateUrl, secretKey);
        });

        connection.On<string>("OnForceUpgrade", async (message) =>
        {
            Console.WriteLine($"[推送] 收到强制更新通知: {message}");
            // 强制更新 - 不询问用户直接开始
            await StartUpdateAsync(updateUrl, secretKey);
        });

        // 3. 启动连接
        try
        {
            await connection.StartAsync();
            Console.WriteLine("[推送] 已连接到更新推送服务");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[推送] 连接失败: {ex.Message}");
        }

        // 4. 保持应用运行
        Console.WriteLine("[推送] 等待服务端推送更新...");
        await Task.Delay(Timeout.Infinite);
    }

    private static async Task StartUpdateAsync(string updateUrl, string secretKey)
    {
        try
        {
            var bootstrap = new GeneralUpdateBootstrap()
                .SetSource(updateUrl, secretKey)
                .SetOption(Option.AppType, AppType.Client)
                .SetOption(Option.BackupEnabled, true)
                .AddListenerMultiDownloadStatistics((_, e) =>
                    Console.WriteLine($"[推送更新] 下载: {e.ProgressValue}%"))
                .AddListenerMultiDownloadCompleted((_, e) =>
                    Console.WriteLine($"[推送更新] 下载完成"))
                .AddListenerException((_, e) =>
                    Console.WriteLine($"[推送更新] 错误: {e.Message}"));

            await bootstrap.LaunchAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[推送更新] 启动失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 安全的 HubConnection 包装器（修复 Dispose 后重连崩溃问题）
/// </summary>
public class SafeHubConnection : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _hubUrl;

    public SafeHubConnection(string hubUrl)
    {
        _hubUrl = hubUrl;
    }

    public async Task StartAsync()
    {
        // 如果已释放，重新创建
        if (_connection == null)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect()
                .Build();
        }

        if (_connection.State != HubConnectionState.Connected)
        {
            await _connection.StartAsync();
        }
    }

    public async Task StopAsync()
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.StopAsync();
        }
    }

    public HubConnectionState? State => _connection?.State;

    public IDisposable On(string methodName, Action<string> handler)
    {
        return _connection?.On(methodName, handler) ?? throw new InvalidOperationException("Connection not initialized");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null; // 必须置 null，否则重连时崩溃
        }
    }
}
