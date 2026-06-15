using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// 【Skill 自动生成】命名管道 IPC（替代加密文件 IPC）
///
/// GeneralUpdate 默认使用 AES 加密文件进行 Client → Upgrade 的 IPC 通信。
/// 在某些场景下（如安全要求高、反病毒软件干扰文件访问），
/// 可以使用命名管道（NamedPipe）替代文件 IPC。
///
/// 优势：
/// - 无磁盘文件写入（安全性更高）
/// - 无 TOCTOU 攻击风险
/// - 无防病毒软件干扰
/// - 支持双向通信
///
/// 注意：此实现需要自行集成到 GeneralUpdateBootstrap 的流程中。
/// 当前 GeneralUpdate 默认使用 EncryptedFileProcessContractProvider，
/// 可通过 IProcessInfoProvider 接口替换。
///
/// NuGet: 无需额外包（System.IO.Pipes 在 .NET 内置）
/// </summary>
public class NamedPipeIpcProvider : IAsyncDisposable
{
    private const string PipeName = "GeneralUpdate_IPC_" + /* ProcessId */ "";
    private NamedPipeServerStream? _server;
    private NamedPipeClientStream? _client;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// 由 Client 进程调用：创建服务端管道，等待 Upgrade 进程连接
    /// </summary>
    public async Task<string> ServerWaitAsync(int timeoutMs = 30000)
    {
        _server = new NamedPipeServerStream(
            PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            TransmissionMode.Byte,
            PipeOptions.Asynchronous);

        // 等待 Upgrade 进程连接
        await _server.WaitForConnectionAsync(_cts.Token);
        return PipeName; // 返回管道名，通过环境变量传给 Upgrade 进程
    }

    /// <summary>
    /// 由 Upgrade 进程调用：连接到 Client 创建的管道
    /// </summary>
    public async Task ClientConnectAsync(string pipeName, int timeoutMs = 30000)
    {
        _client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await _client.ConnectAsync(timeoutMs, _cts.Token);
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    public async Task SendAsync<T>(T data)
    {
        var stream = _server ?? _client as Stream
            ?? throw new InvalidOperationException("未连接");

        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);

        // 先发长度，再发内容
        await stream.WriteAsync(lengthBytes, _cts.Token);
        await stream.WriteAsync(bytes, _cts.Token);
        await stream.FlushAsync(_cts.Token);
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    public async Task<T?> ReceiveAsync<T>()
    {
        var stream = _server ?? _client as Stream
            ?? throw new InvalidOperationException("未连接");

        // 先读长度
        var lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer, _cts.Token);
        var length = BitConverter.ToInt32(lengthBuffer);

        // 再读内容
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = await stream.ReadAsync(
                buffer, offset, length - offset, _cts.Token);
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
