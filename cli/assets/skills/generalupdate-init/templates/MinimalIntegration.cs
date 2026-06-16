using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;

/// <summary>
/// GeneralUpdate 最小集成示例
///
/// 这 3 行代码包含了完整的更新流程：
/// 版本验证 → 下载 → IPC → 启动升级进程 → 替换文件 → 重启
///
/// 使用条件（必须是满足以下所有条件）：
/// 1. 项目发布目录存在 generalupdate.manifest.json（自动发现应用元数据）
/// 2. UpgradeApp.exe 已放置在 InstallPath/update/ 子目录中
/// 3. 后端已部署 GeneralSpacestation 或兼容 API
/// 4. 当前应用为主程序，另有一个独立的升级程序
///
/// 如果以上条件不满足，请使用 FullIntegration.cs 显式配置。
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// ⚠️ 针对 NuGet v10.4.6 稳定版
/// </summary>
public static class MinimalIntegration
{
    public static async Task RunAsync()
    {
        // 1. 创建配置对象
        var config = new Configinfo
        {
            UpdateUrl = "{{UPDATE_URL}}",
            AppSecretKey = "{{APP_SECRET_KEY}}",
            AppName = "{{PROJECT_NAME}}.exe",
            MainAppName = "{{PROJECT_NAME}}.exe",
            ClientVersion = "{{CLIENT_VERSION}}",
            ProductId = "{{PRODUCT_ID}}",
            InstallPath = "{{INSTALL_PATH}}"
        };

        // 2. 启动更新
        // LaunchAsync 返回 bootstrap 实例。
        // 有更新 → 更新完成后当前进程退出（由 Upgrade 进程重启）
        // 无更新 → 继续执行
        await new GeneralUpdateBootstrap()
            .SetConfig(config)
            .LaunchAsync();
    }
}
