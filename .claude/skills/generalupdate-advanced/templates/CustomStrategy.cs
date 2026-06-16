using GeneralUpdate.Common.Internal.Pipeline;

/// <summary>
/// 【Skill 参考】自定义平台策略
///
/// ⚠️ 注意：v10.4.6 稳定版不支持通过 bootstrap.Strategy<T>() 注入自定义策略。
/// 自定义策略需要直接继承 AbstractStrategy 并手动调用。
///
/// AbstractStrategy 模板方法：
/// - Create(UpdateContext) — 初始化
/// - ExecuteAsync() — 执行策略主体
/// - StartAppAsync() — 启动主应用
/// - BuildPipeline(PipelineContext) — 构建平台特定中间件链
/// </summary>
public class MyCustomStrategy
{
    // v10.4.6 稳定版不支持自定义策略注入
    // 此模板作为开发分支（v10.5.0-beta.2）特性的参考

    public static async Task ExamplePipelineAsync()
    {
        var context = new PipelineContext();
        context.Add("ZipFilePath", @"C:\temp\update.zip");
        context.Add("Hash", "");
        context.Add("Format", 0);
        context.Add("Encoding", System.Text.Encoding.UTF8);
        context.Add("SourcePath", @"C:\Program Files\MyApp");
        context.Add("PatchEnabled", true);

        await new PipelineBuilder(context)
            .UseMiddleware<HashMiddleware>()
            .UseMiddleware<CompressMiddleware>()
            .Build();

        Console.WriteLine("[CustomStrategy] 管道执行完成");
    }
}
