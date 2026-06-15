using GeneralUpdate.Bowl;
using GeneralUpdate.Core.Models;

/// <summary>
/// 【Skill 自动生成】Bowl 崩溃守护集成
///
/// Bowl 是一个跨平台的崩溃监控助手，在升级完成后监控主应用的启动情况。
/// 如果主应用崩溃，Bowl 会：
/// 1. 生成 MiniDump 文件
/// 2. 写入 CrashReport.json（崩溃诊断报告）
/// 3. 可选：从备份自动回滚
/// 4. 触发 OnCrash 回调
///
/// 使用方式：
///   在 Upgrade 完成后启动 Bowl（由 StartAppAsync 自动处理）
///   或者在主应用启动后手动调用 Bowl.LaunchAsync()
///
/// NuGet: dotnet add package GeneralUpdate.Bowl
///
/// ⚠️ 注意事项：
/// 1. Bowl 目前仅在 Windows 上充分测试
/// 2. 回滚依赖于更新前的备份（BackupEnabled = true）
/// 3. 备份保留最多 3 个版本
/// 4. Bowl 需要使用 procdump 工具（Windows）
/// </summary>
public static class BowlIntegration
{
    /// <summary>
    /// 启动 Bowl 崩溃守护
    /// </summary>
    public static async Task StartBowlAsync(string appPath, string installPath)
    {
        Console.WriteLine("[Bowl] 启动崩溃守护进程...");

        var bowl = new Bowl();

        // 注册崩溃回调
        bowl.OnCrash += (crashReport) =>
        {
            Console.WriteLine($"[Bowl] 检测到崩溃!");
            Console.WriteLine($"[Bowl] 原因: {crashReport.CrashReason}");
            Console.WriteLine($"[Bowl] Dump 文件: {crashReport.DumpFilePath}");

            // 自动回滚（前提是有备份）
            if (crashReport.AutoRestore)
            {
                Console.WriteLine("[Bowl] 正在回滚到备份版本...");
                // Bowl 会自动从备份目录恢复
            }
        };

        // 启动监控
        await bowl.LaunchAsync(new BowlOptions
        {
            // 要监控的主应用路径
            TargetAppPath = appPath,
            // 安装目录（用于定位备份）
            InstallPath = installPath,
            // 启用自动回滚
            AutoRestore = true,
            // 崩溃报告输出目录
            ReportOutputPath = Path.Combine(
                installPath, "CrashReports")
        });
    }
}
