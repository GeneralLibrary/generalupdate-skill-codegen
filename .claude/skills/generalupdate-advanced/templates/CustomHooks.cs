using GeneralUpdate.Core;
using GeneralUpdate.Core.Hooks;

/// <summary>
/// 【Skill 参考】自定义生命周期 Hooks （v10.5.0-beta.4 可用）
///
/// 通过 IUpdateHooks 接口可以拦截更新流程的各个阶段：
/// - OnBeforeUpdateAsync: 更新开始前（可取消）
/// - OnDownloadCompletedAsync: 下载完成后
/// - OnAfterUpdateAsync: 更新完成后
/// - OnUpdateErrorAsync: 更新出错时
/// - OnBeforeStartAppAsync: 启动应用前
///
/// 注册方式：
///   new GeneralUpdateBootstrap()
///       .Hooks&lt;MyCustomHooks&gt;()
///       ...
///
/// 内置实现：
/// - NoOpUpdateHooks（默认，空操作）
/// - UnixPermissionHooks（设置 Linux/macOS 可执行权限）
/// - CustomPermissionHooks（自定义权限脚本）
/// </summary>
public class MyCustomHooks : IUpdateHooks
{
    public async Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        Console.WriteLine($"[Hooks] 更新前检查: 当前={ctx.CurrentVersion} → 目标={ctx.TargetVersion}");
        // 返回 false 取消本次更新
        return true;
    }

    public async Task OnDownloadCompletedAsync(DownloadContext ctx)
    {
        Console.WriteLine($"[Hooks] 下载完成: {ctx.AssetName} ({ctx.TotalBytes} bytes)");
    }

    public async Task OnAfterUpdateAsync(HookContext ctx)
    {
        Console.WriteLine($"[Hooks] 更新完成: {ctx.CurrentVersion} → {ctx.TargetVersion}");
    }

    public async Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        Console.WriteLine($"[Hooks] 更新失败: {ex.Message}");
    }

    public async Task OnBeforeStartAppAsync(HookContext ctx)
    {
        Console.WriteLine($"[Hooks] 即将启动应用: {ctx.InstallPath}");
    }
}
