using GeneralUpdate.Core;

Console.WriteLine("[Upgrade] 升级程序启动");

try
{
    // Upgrade 模式从 IPC 读取配置，无需 SetConfig
    await new GeneralUpdateBootstrap()
        .AddListenerException((_, e) =>
            Console.WriteLine($"[Upgrade] 错误: {e.Message}"))
        .LaunchAsync();

    Console.WriteLine("[Upgrade] 更新完成，主程序已启动");
}
catch (Exception ex)
{
    Console.WriteLine($"[Upgrade] 严重错误: {ex}");
    Environment.ExitCode = 1;
}
