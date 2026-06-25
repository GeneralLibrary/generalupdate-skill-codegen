using System.Windows;
using Upgrade.WPF.ViewModels;

namespace Upgrade.WPF.Views;

/// <summary>
/// 【Skill 自动生成】WPF + LayUI.Wpf 更新窗口 Code-Behind
///
/// 使用方式：
///   1. 安装 NuGet: LayUI.Wpf
///   2. 在 App.xaml 中引用 LayUI 主题
///   3. 将 RealDownloadService 适配为 WPF 的 IDownloadService
///   4. 打开窗口：
///      var vm = new LayUIUpdateViewModel(downloadService);
///      var window = new UpdateWindow { DataContext = vm };
///      window.ShowDialog();
/// </summary>
public partial class UpdateWindow
{
    public UpdateWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 便捷构造：直接传入已配置的 ViewModel。
    /// </summary>
    public UpdateWindow(LayUIUpdateViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
