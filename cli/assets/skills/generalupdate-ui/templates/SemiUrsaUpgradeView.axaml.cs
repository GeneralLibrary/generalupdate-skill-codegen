using Avalonia.Controls;
using Upgrade.Avalonia.ViewModels;

namespace Upgrade.Avalonia.Views;

/// <summary>
/// 【Skill 自动生成】Avalonia + SemiUrsa 升级进程窗口 Code-Behind
///
/// Upgrade 进程在应用更新时显示"正在更新，请稍候"界面。
/// 使用方式：
///   var window = new UpdateWindow();
///   window.Show();
/// </summary>
public partial class UpdateWindow : Window
{
    public UpdateWindow()
    {
        InitializeComponent();
    }

    public UpdateWindow(UpdateViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
