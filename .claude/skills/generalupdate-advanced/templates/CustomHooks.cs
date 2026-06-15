using GeneralUpdate.Core.Hooks;
using GeneralUpdate.Core.Models;

/// <summary>
/// 【Skill 自动生成】自定义生命周期 Hooks
///
/// 实现 IUpdateHooks 接口，在更新的各个生命周期阶段插入自定义逻辑。
/// 所有方法都有默认实现（返回 null/true），只需重写需要的方法。
///
/// 使用方式：
///   .Hooks<MyCustomHooks>()
/// </summary>
public class MyCustomHooks : IUpdateHooks
{
    /// <summary>
    /// 更新开始前调用。返回 false 中止更新。
    /// 可用于：检查磁盘空间、检查是否在营业时间、用户确认等。
    /// </summary>
    public async Task<bool> OnBeforeUpdateAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] 开始更新: {context.CurrentVersion} → {context.LastVersion}");

        // 检查磁盘空间
        var drive = new DriveInfo(Path.GetPathRoot(context.InstallPath)!);
        if (drive.AvailableFreeSpace < 100 * 1024 * 1024) // 100MB 最低要求
        {
            Console.WriteLine("[Hooks] 磁盘空间不足，中止更新");
            return false;
        }

        return true; // true = 继续更新
    }

    /// <summary>
    /// 下载完成后调用（在 Client 进程）
    /// 可用于：下载后扫描、日志记录、通知 UI
    /// </summary>
    public async Task OnDownloadCompletedAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] 下载完成: {context.LastVersion}");
        // 可以在这里触发 UI 通知
        await Task.CompletedTask;
    }

    /// <summary>
    /// 更新完成后调用（在 Upgrade 进程，替换文件后）
    /// 可用于：清理临时文件、更新数据库 schema、迁移用户配置
    /// </summary>
    public async Task OnAfterUpdateAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] 更新完成: {context.LastVersion}");

        // 清理临时文件
        var tempDir = context.UpdatePath;
        if (Directory.Exists(tempDir))
        {
            try { Directory.Delete(tempDir, true); }
            catch { /* 忽略清理中的错误 */ }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 更新过程出错时调用
    /// 可用于：错误日志、通知管理员、触发回滚
    /// </summary>
    public async Task OnUpdateErrorAsync(UpdateContext context, Exception exception)
    {
        Console.WriteLine($"[Hooks] 更新失败: {exception.Message}");
        // 记录错误日志
        File.WriteAllText(
            Path.Combine(context.InstallPath, "update_error.log"),
            $"[{DateTime.UtcNow}] {exception}");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 启动主应用前调用（在 Upgrade 进程）
    /// 可用于：修改配置文件、设置环境变量、检查版本兼容性
    /// 返回 false 阻止主应用启动
    /// </summary>
    public async Task<bool> OnBeforeStartAppAsync(UpdateContext context)
    {
        Console.WriteLine($"[Hooks] 准备启动主应用: {context.MainAppName}");

        // 在 Linux/MacOS 上设置可执行权限
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var appPath = Path.Combine(context.InstallPath, context.MainAppName ?? "");
            if (File.Exists(appPath))
            {
                await UnixPermissionHooks.SetExecutablePermissionAsync(appPath);
            }
        }

        return true; // true = 启动主应用
    }
}
