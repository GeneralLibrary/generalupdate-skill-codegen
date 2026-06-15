using System.IO.Pipes;
using System.Text;
using System.Text.Json;

/// <summary>
/// [Skill Generated] NamedPipe IPC — replaces encrypted file IPC.
///
/// Advantages over default encrypted file IPC:
/// - No disk writes (more secure)
/// - No TOCTOU attack risk
/// - No antivirus interference
/// - Bidirectional communication
///
/// The default GeneralUpdate IPC uses AES-encrypted files at
/// %TEMP%/GeneralUpdate/ipc/process_info.enc. For environments
/// with high security requirements or antivirus interference,
/// NamedPipe provides a safer alternative.
///
/// Integration: Wire into IProcessInfoProvider to replace the default.
/// </summary>
public class NamedPipeIpcProvider : IAsyncDisposable
{
    // Unique pipe name per client process — prevents collisions between parallel instances
    private readonly string _pipeName = $"GeneralUpdate_IPC_{Environment.ProcessId}";
    private NamedPipeServerStream? _server;
    private NamedPipeClientStream? _client;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>Called by Client process: create server pipe, wait for Upgrade.</summary>
    public async Task<string> ServerWaitAsync(int timeoutMs = 30000)
    {
        _server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut,
            maxNumberOfServerInstances: 1, TransmissionMode.Byte, PipeOptions.Asynchronous);

        // Enforce caller-supplied timeout via CancellationTokenSource
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, _cts.Token);
        try
        {
            await _server.WaitForConnectionAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException($"Upgrade process did not connect within {timeoutMs}ms");
        }
        return _pipeName;
    }

    /// <summary>Called by Upgrade process: connect to Client pipe.</summary>
    public async Task ClientConnectAsync(string pipeName, int timeoutMs = 30000)
    {
        _client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await _client.ConnectAsync(timeoutMs, _cts.Token);
    }

    /// <summary>Send serialized data through the pipe.</summary>
    public async Task SendAsync<T>(T data)
    {
        var stream = _server ?? _client as Stream ?? throw new InvalidOperationException("Not connected");
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);
        await stream.WriteAsync(lengthBytes, _cts.Token);
        await stream.WriteAsync(bytes, _cts.Token);
        await stream.FlushAsync(_cts.Token);
    }

    /// <summary>Receive deserialized data from the pipe.</summary>
    public async Task<T?> ReceiveAsync<T>()
    {
        var stream = _server ?? _client as Stream ?? throw new InvalidOperationException("Not connected");
        var lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer, _cts.Token);
        var length = BitConverter.ToInt32(lengthBuffer);
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = await stream.ReadAsync(buffer, offset, length - offset, _cts.Token);
            offset += read;
        }
        var json = Encoding.UTF8.GetString(buffer);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _server?.Dispose();
        _client?.Dispose();
    }
}
