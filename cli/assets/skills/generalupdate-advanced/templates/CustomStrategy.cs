using GeneralUpdate.Core;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Core.Hooks;
using GeneralUpdate.Core.Strategy;
using GeneralUpdate.Core.Download.Reporting;

/// <summary>
/// 【Skill 参考】自定义平台策略（v10.5.0-beta.6 可用）
///
/// 通过 bootstrap.Strategy&lt;T&gt;() 注入自定义策略。
/// 自定义策略需要实现 IStrategy 接口。
///
/// 也可使用 PipelineBuilder 构建自定义中间件链。
/// </summary>
public class MyCustomStrategy
{
    // ========== 方式一：通过 Strategy&lt;T&gt;() 注入 ==========

    /// <summary>
    /// 自定义策略示例。注册方式：
    ///   new GeneralUpdateBootstrap()
    ///       .Strategy&lt;MyCustomUpdateStrategy&gt;()
    ///       .SetConfig(config)
    ///       .LaunchAsync()
    /// </summary>
    public class MyCustomUpdateStrategy : IStrategy
    {
        public IUpdateHooks Hooks { get; set; } = new NoOpUpdateHooks();
        public IUpdateReporter Reporter { get; set; } = new HttpUpdateReporter();

        private UpdateContext _context = default!;

        public void Create(UpdateContext parameter)
        {
            _context = parameter;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("[CustomStrategy] 执行自定义更新逻辑");
            // 自定义更新实现
        }

        public async Task StartAppAsync()
        {
            Console.WriteLine("[CustomStrategy] 启动应用");
            // 自定义启动逻辑
        }
    }

    // ========== 方式二：PipelineBuilder 管道 ==========

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
