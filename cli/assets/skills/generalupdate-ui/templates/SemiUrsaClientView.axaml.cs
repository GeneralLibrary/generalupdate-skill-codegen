using Avalonia.Controls;
using Client.Avalonia.ViewModels;

namespace Client.Avalonia.Views;

/// <summary>
/// 【Skill 自动生成】Avalonia + SemiUrsa 客户端更新窗口 Code-Behind
///
/// 在构造函数中注入 IDownloadService → EnhancedDownloadViewModel。
/// 使用方式：
///   1. 在 App.axaml.cs 中注册 DI：
///      services.AddSingleton&lt;IDownloadService&gt;(sp =&gt;
///          new RealDownloadService("https://your-server.com/api", "your-key"));
///      services.AddTransient&lt;EnhancedDownloadViewModel&gt;();
///   2. 打开窗口：
///      var vm = app.Services.GetRequiredService&lt;EnhancedDownloadViewModel&gt;();
///      var window = new UpdateWindow { DataContext = vm };
///      window.Show();
/// </summary>
public partial class UpdateWindow : Window
{
    public UpdateWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 便捷构造：直接传入已配置的 ViewModel。
    /// </summary>
    public UpdateWindow(EnhancedDownloadViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
