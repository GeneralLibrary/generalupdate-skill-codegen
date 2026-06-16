using GeneralUpdate.Bowl;
using GeneralUpdate.Bowl.Strategys;

/// <summary>
/// 【Skill 参考】Bowl 崩溃守护
///
/// ⚠️ 注意：v10.4.6 稳定版中 Bowl 仅提供基础类型定义。
/// Bowl.LaunchAsync() 等完整功能在开发分支（v10.5.0-beta.2）中可用。
///
/// 此模板展示 v10.4.6 的实际 API 调用方式。
///
/// NuGet: dotnet add package GeneralUpdate.Bowl
/// </summary>
public static class BowlIntegration
{
    public static void ConfigureBowl()
    {
        // v10.4.6 中的 Bowl API：
        // Bowl 类有公开构造函数，但无公开 LaunchAsync 方法
        // 完整崩溃守护功能请关注后续版本

        var param = new MonitorParameter
        {
            ProcessNameOrId = "MyApp.exe",
            DumpFileName = "v1.0.0.0_fail.dmp",
            FailFileName = "v1.0.0.0_fail.json",
            TargetPath = @"C:\Program Files\MyApp",
            FailDirectory = @"C:\Program Files\MyApp\fail",
            BackupDirectory = @"C:\Program Files\MyApp\backup",
            WorkModel = "Upgrade",
        };

        var bowl = new Bowl();
        Console.WriteLine("[Bowl] Bowl 实例已创建。完整监控功能需 v10.5+。");
    }
}
