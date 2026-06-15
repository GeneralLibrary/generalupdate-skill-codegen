using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

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
/// 4. 当前应用为主程序（AppType.Client），另有一个独立的升级程序
///
/// 如果以上条件不满足，请使用 FullIntegration.cs 显式配置。
///
/// NuGet: dotnet add package GeneralUpdate.Core
/// </summary>
public static class MinimalIntegration
{
    public static async Task<bool> RunAsync()
    {
        // 三行核心代码
        var bootstrap = new GeneralUpdateBootstrap()
            .SetSource(
                "https://your-server.com/api",     // UpdateUrl
                "your-32-char-secret-key-here!"    // AppSecretKey
            )
            .SetOption(Option.AppType, AppType.Client);

        // LaunchAsync 返回：
        //   true  = 有更新 → 已完成下载 → 已启动 Upgrade 进程 → 当前进程即将退出
        //   false = 已是最新版本 → 直接进入主程序逻辑
        //   throw = 更新过程中发生不可恢复的错误
        return await bootstrap.LaunchAsync();
    }
}

/* ═══════════════════════════════════════════════════
 * generalupdate.manifest.json — 必须放在发布目录根目录
 *
 * ⚠️ 常见错误（来自 Issue #484、#475、#501）：
 * 1. ClientVersion 为空 → 默认为 "0.0.0.0" → 每次都认为有新版本 → 无限更新
 * 2. "MainAppName" 拼写错误（大小写敏感）
 * 3. 文件放错目录（必须和 .exe 同级）
 * 4. UpdatePath 写成了相对路径但当前目录不对
 *
 * ⚠️ Version format: Must use 4 segments, e.g. "1.0.0.0"
 *   new Version("1.0") parses to 1.0.-1.-1 (Build=-1, Revision=-1),
 *   NOT 1.0.0.0. When the server returns 1.0.0.0 (Build=0, Revision=0),
 *   new Version("1.0") < new Version("1.0.0.0") is TRUE,
 *   causing a false "update available" detection → infinite upgrade loop.
 *   ALWAYS use 4-segment version strings everywhere.
 *
 * ⚠️ 路径规则：
 *   InstallPath: 可写 "."（表示 exe 所在目录）或绝对路径
 *   UpdatePath:  相对于 InstallPath，或绝对路径
 *     例如 InstallPath="C:\App", UpdatePath="update"
 *     → Upgrade.exe 应该在 C:\App\update\UpgradeApp.exe
 *
 * ⚠️ UpgradeApp.exe 必须是随首个版本一起发布的独立可执行文件！
 *   GeneralUpdate 不能自己下载 "升级程序" 文件（先有鸡还是先有蛋问题）
 *   唯一的例外是：Upgrade 程序可以通过 Client 进程"原地更新"自身
 *   （UpgradeOnly 场景：下载 Upgrade 包 → 解压覆盖 UpgradeApp.exe）
 ═══════════════════════════════════════════════════ */
/*
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

/* ═══════════════════════════════════════════════════
 * 双进程目录结构示例
 *
 * 首次发布（v1.0.0.0）：
 *   C:\Program Files\MyApp\
 *   ├── MyApp.exe                 ← 主程序 (AppType.Client)
 *   ├── MyApp.dll
 *   ├── generalupdate.manifest.json
 *   └── update\
 *       └── UpgradeApp.exe        ← 升级程序 (AppType.Upgrade)
 *
 * 更新后（v1.1.0.0）：
 *   C:\Program Files\MyApp\
 *   ├── MyApp.exe                 ← 被 UpgradeApp 替换为新版
 *   ├── MyApp.dll
 *   ├── generalupdate.manifest.json  ← ClientVersion 已回写为 1.1.0.0
 *   └── update\
 *       └── UpgradeApp.exe        ← 可能也被替换为新版（Both 场景）
 ═══════════════════════════════════════════════════ */
