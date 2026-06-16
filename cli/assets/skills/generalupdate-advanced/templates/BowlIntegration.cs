using System;
using System.Threading.Tasks;
using GeneralUpdate.Bowl;

/// <summary>
/// 【Skill 参考】Bowl 崩溃守护
///
/// v10.5.0-beta.4 中 Bowl 使用 BowlContext 配置，支持 LaunchAsync 方法。
///
/// NuGet: dotnet add package GeneralUpdate.Bowl --version 10.5.0-beta.4
/// </summary>
public static class BowlIntegration
{
    public static async Task RunBowlAsync()
    {
        var context = new BowlContext
        {
            ProcessNameOrId = "MyApp.exe",
            DumpFileName = "v1.0.0.0_fail.dmp",
            FailFileName = "v1.0.0.0_fail.json",
            TargetPath = @"C:\Program Files\MyApp",
            FailDirectory = @"C:\Program Files\MyApp\fail",
            BackupDirectory = @"C:\Program Files\MyApp\backup",
            WorkModel = "Upgrade",
            TimeoutMs = 30_000,
            AutoRestore = true,
        };

        var bowl = new Bowl();
        var result = await bowl.LaunchAsync(context);

        Console.WriteLine($"[Bowl] 监控完成: Success={result.Success}, DumpCaptured={result.DumpCaptured}");
    }
}
