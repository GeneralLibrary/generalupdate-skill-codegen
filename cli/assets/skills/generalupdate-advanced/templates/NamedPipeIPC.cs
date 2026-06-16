using System.IO.Pipes;
using System.Text;
using System.Text.Json;

/// <summary>
/// 【Skill 参考】命名管道 IPC
///
/// ⚠️ 注意：v10.4.6 稳定版中不存在 IProcessInfoProvider 接口。
/// IPC 实现在当前版本中不可替换。
///
/// 此代码作为 NamedPipe 通信模式的参考实现，
/// 实际替换 IPC 需要 v10.5.0-beta.2 开发分支。
/// </summary>
public class NamedPipeIpcProvider : IAsyncDisposable
{
    private const string PipeNamePrefix = "GeneralUpdate_IPC_";
    private NamedPipeServerStream? _server;
    private NamedPipeClientStream? _client;
    private readonly CancellationTokenSource _cts = new();

    public async Task<string> ServerWaitAsync(int processId, int timeoutMs = 30000)
    {
        var pipeName = PipeNamePrefix + processId;
        _server = new NamedPipeServerStream(
            pipeName, PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);
        try
        {
            await _server.WaitForConnectionAsync(linkedCts.Token);
            return pipeName;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException($"NamedPipe 等待连接超时 ({timeoutMs}ms)");
        }
    }

    public async Task ClientConnectAsync(string pipeName, int timeoutMs = 30000)
    {
        _client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await _client.ConnectAsync(timeoutMs, _cts.Token);
    }

    public async Task SendAsync<T>(T data)
    {
        var stream = GetStream();
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);
        if (!BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
        await stream.WriteAsync(lengthBytes, _cts.Token);
        await stream.WriteAsync(bytes, _cts.Token);
        await stream.FlushAsync(_cts.Token);
    }

    public async Task<T?> ReceiveAsync<T>()
    {
        var stream = GetStream();
        var lengthBuffer = new byte[4];
        var lengthRead = 0;
        while (lengthRead < 4)
        {
            var read = await stream.ReadAsync(lengthBuffer, lengthRead, 4 - lengthRead, _cts.Token);
            if (read == 0) throw new EndOfStreamException("管道连接已关闭");
            lengthRead += read;
        }
        // 先处理大端序，再解析长度
        if (!BitConverter.IsLittleEndian) Array.Reverse(lengthBuffer);
        var length = BitConverter.ToInt32(lengthBuffer);
        if (length <= 0 || length > 10 * 1024 * 1024) // 限制最大 10MB
            throw new InvalidDataException($"无效的消息长度: {length}");
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = await stream.ReadAsync(buffer, offset, length - offset, _cts.Token);
            if (read == 0) throw new EndOfStreamException("管道连接已关闭");
            offset += read;
        }
        var json = Encoding.UTF8.GetString(buffer);
        return JsonSerializer.Deserialize<T>(json);
    }

    private Stream GetStream()
    {
        if (_server?.IsConnected == true) return _server;
        if (_client?.IsConnected == true) return _client;
        throw new InvalidOperationException("NamedPipe 未连接");
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _server?.Dispose();
        _client?.Dispose();
        _cts.Dispose();
    }
}
