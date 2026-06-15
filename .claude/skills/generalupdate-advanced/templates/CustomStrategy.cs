using GeneralUpdate.Core.Strategy;
using GeneralUpdate.Core.Pipeline;

/// <summary>
/// 【Skill 自动生成】自定义平台更新策略
///
/// 完全替换 GeneralUpdate 的默认策略（WindowsStrategy / LinuxStrategy / MacStrategy）。
/// 适用于需要完全控制更新行为的场景。
///
/// 使用方式：
///   .Strategy<MyCustomStrategy>()
///
/// IStrategy 接口主要方法：
/// - ExecuteAsync(UpdateContext)：执行策略主体
/// - StartAppAsync(UpdateContext)：启动主应用
/// </summary>
public class MyCustomStrategy : AbstractStrategy
{
    /// <summary>
    /// 自定义策略入口
    /// </summary>
    public override async Task ExecuteAsync(UpdateContext context)
    {
        Console.WriteLine("[CustomStrategy] 执行自定义更新策略");

        // 1. 前置检查
        if (await Hooks.SafeOnBeforeUpdateAsync(context) == false)
        {
            Console.WriteLine("[CustomStrategy] 前置检查未通过，中止更新");
            return;
        }

        // 2. 对每个版本执行管道
        foreach (var version in context.UpdateVersions)
        {
            Console.WriteLine($"[CustomStrategy] 处理版本: {version.Version}");

            // 2a. 构建管道（可自定义）
            var pipeline = new PipelineBuilder(context)
                .UseMiddleware<HashMiddleware>()         // SHA256 校验
                .UseMiddleware<CompressMiddleware>()     // 解压 ZIP
                .Build();

            // 2b. 执行管道
            await pipeline.ExecuteAsync(context, version);

            Console.WriteLine($"[CustomStrategy] 版本 {version.Version} 处理完成");
        }

        // 3. 后置处理
        await Hooks.SafeOnAfterUpdateAsync(context);

        // 4. 启动主应用
        await StartAppAsync(context);
    }

    /// <summary>
    /// 启动主应用
    /// </summary>
    public override async Task StartAppAsync(UpdateContext context)
    {
        Console.WriteLine("[CustomStrategy] 启动主应用");

        var appPath = Path.Combine(
            context.InstallPath,
            context.MainAppName ?? "MyApp.exe");

        if (!File.Exists(appPath))
        {
            throw new FileNotFoundException(
                $"主应用不存在: {appPath}");
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = appPath,
            WorkingDirectory = context.InstallPath,
            UseShellExecute = true
        });

        if (process == null)
        {
            throw new InvalidOperationException(
                $"无法启动主应用: {appPath}");
        }

        Console.WriteLine($"[CustomStrategy] 主应用已启动 (PID: {process.Id})");
    }

    /// <summary>
    /// 实现原样退出
    /// </summary>
    public override async Task ExecuteAsync(UpdateContext context, string pipeHandle)
    {
        await ExecuteAsync(context);
    }
}
