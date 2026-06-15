using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

/// <summary>
/// GeneralUpdate 最小集成示例 — 三行代码搞定自动更新
///
/// 使用条件：
/// 1. 项目发布目录存在 generalupdate.manifest.json（自动发现元数据）
/// 2. UpgradeApp.exe 已放置在安装目录的 update/ 子目录中
/// 3. 后端已部署 GeneralSpacestation 或兼容 API
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class MinimalIntegration
{
    public static async Task<bool> RunAsync()
    {
        var result = await new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-server.com/api",     // UpdateUrl
                "your-32-char-secret-key-here!"     // AppSecretKey
            )
            .SetOption(Option.AppType, AppType.Client)
            .LaunchAsync();

        // result = true  → 更新已应用，应用将重启
        // result = false → 已是最新版本
        return result;
    }
}

/* generalupdate.manifest.json — 放在发布目录根目录
{
  "MainAppName": "MyApp.exe",
  "UpdateAppName": "UpgradeApp.exe",
  "ProductId": "my-product-001",
  "InstallPath": ".",
  "UpdatePath": "update",
  "ClientVersion": "1.0.0.0",
  "UpgradeClientVersion": "1.0.0.0"
}
*/
