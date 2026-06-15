using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Strategy 6: SignalR Push update.
/// Server actively pushes update notifications to clients.
///
/// Flow:
/// Client connects to SignalR Hub -> Admin uploads new package
/// -> Clicks "Push" -> Hub notifies all clients -> Bootstrap starts update
///
/// NuGet:
///   dotnet add package GeneralUpdate.Core
///   dotnet add package Microsoft.AspNetCore.SignalR.Client
///
/// Known issue (#402, Code Audit #5):
/// UpgradeHubService.DisposeAsync does not null the connection reference
/// -> ObjectDisposedException on reconnect. Use SafeHubConnection wrapper.
/// </summary>
public static class PushStrategy
{
    public static async Task RunAsync()
    {
        var updateUrl = "https://your-server.com/api";
        var secretKey = "your-secret-key";
        var hubUrl = "https://your-server.com/hub/upgrade";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("OnPushUpgrade", async (message) =>
        {
            Console.WriteLine($"[Push] Update notification: {message}");
            await StartUpdateAsync(updateUrl, secretKey);
        });

        connection.On<string>("OnForceUpgrade", async (message) =>
        {
            Console.WriteLine($"[Push] Force update: {message}");
            await StartUpdateAsync(updateUrl, secretKey);
        });

        try
        {
            await connection.StartAsync();
            Console.WriteLine("[Push] Connected to update push service");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Connection failed: {ex.Message}");
        }

        Console.WriteLine("[Push] Waiting for server push...");
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
                    Console.WriteLine($"[Push/Update] Download: {e.ProgressValue}%"))
                .AddListenerMultiDownloadCompleted((_, e) =>
                    Console.WriteLine($"[Push/Update] Download complete"))
                .AddListenerException((_, e) =>
                    Console.WriteLine($"[Push/Update] Error: {e.Message}"));

            await bootstrap.LaunchAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push/Update] Failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Safe HubConnection wrapper — fixes ObjectDisposedException on reconnect.
/// UpgradeHubService.DisposeAsync does not null the connection reference.
/// This wrapper ensures null after Dispose, allowing clean reconnect.
/// </summary>
public class SafeHubConnection : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _hubUrl;

    public SafeHubConnection(string hubUrl) { _hubUrl = hubUrl; }

    public async Task StartAsync()
    {
        if (_connection == null)
            _connection = new HubConnectionBuilder().WithUrl(_hubUrl).WithAutomaticReconnect().Build();
        if (_connection.State != HubConnectionState.Connected)
            await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.StopAsync();
    }

    public HubConnectionState? State => _connection?.State;

    public IDisposable On(string methodName, Action<string> handler)
    {
        return _connection?.On(methodName, handler) ?? throw new InvalidOperationException("Not connected");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;  // CRITICAL: must null to prevent ObjectDisposedException on reconnect
        }
    }
}
