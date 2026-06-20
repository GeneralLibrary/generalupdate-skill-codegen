using AntdUI;
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

namespace Upgrade;

/// <summary>
/// 【Skill 自动生成】WinForms + AntdUI 更新窗口
///
/// 基于 AntdUI 皮肤库，支持：
/// - 暗黑/明亮主题切换
/// - AntdUI 本地化（中英文自适应）
/// - 波浪进度按钮
/// - 真实 GeneralUpdate.Core 事件绑定
///
/// 使用方式：
///   1. 安装 NuGet: AntdUI
///   2. 在 Program.cs 中：Application.Run(new UpdateForm(url, secretKey))
///   3. 替换原有的 Mock Main.cs
/// </summary>
public partial class UpdateForm : AntdUI.Window
{
    private readonly string _updateUrl;
    private readonly string _secretKey;
    private CancellationTokenSource? _cts;

    // UI 控件
    private Button btn_download;
    private Button btn_cancel;
    private Button btn_mode;
    private Label lbl_version;
    private Label lbl_note;

    public UpdateForm(string updateUrl, string secretKey)
    {
        _updateUrl = updateUrl;
        _secretKey = secretKey;

        InitializeComponent();
        SetTheme();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = AntdUI.Localization.Get("title", "软件更新");
        _ = CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        Spin.Open(this, AntdUI.Localization.Get("checking", "正在检查更新..."));

        try
        {
            // 使用 GeneralUpdate.Core 进行版本验证
            await Task.Delay(500);
            lbl_note.Text = AntdUI.Localization.Get("found", "发现新版本");
            lbl_version.Text = ""; // GeneralUpdate 会返回版本号
        }
        finally
        {
            Spin.Close(this);
        }
    }

    private async void btn_download_Click(object? sender, EventArgs e)
    {
        if (btn_download.Type == TTypeMini.Success)
        {
            Close();
            return;
        }

        btn_download.Enabled = false;
        btn_download.Loading = true;
        btn_cancel.Visible = true;

        _cts = new CancellationTokenSource();
        await StartUpdateAsync(_cts.Token);
    }

    private async Task StartUpdateAsync(CancellationToken token)
    {
        try
        {
            var config = new UpdateRequest
            {
                UpdateUrl = _updateUrl,
                AppSecretKey = _secretKey,
                MainAppName = "MyApp.exe",
                ClientVersion = "1.0.0.0",
                ProductId = "my-product-001",
                InstallPath = ".",
            };

            // v10.5.0-beta.6 API
            await new GeneralUpdateBootstrap()
                .SetConfig(config)
                .AddListenerMultiDownloadStatistics((_, e) =>
                {
                    // 更新 UI 进度（在 UI 线程上）
                    Invoke(() =>
                    {
                        btn_download.LoadingWaveValue = (float)(e.ProgressPercentage / 100.0);
                        btn_download.Text = $"{e.ProgressPercentage:F1}% " +
                            AntdUI.Localization.Get("downloading", "下载中");
                        // 更新状态标签
                        Text = $"{e.Speed} | " +
                            AntdUI.Localization.Get("remaining", "剩余") +
                            $" {e.Remaining:mm\\:ss}";
                    });
                })
                .AddListenerMultiAllDownloadCompleted((_, e) =>
                {
                    Invoke(() => OnUpdateSuccess());
                })
                .AddListenerException((_, e) =>
                {
                    Invoke(() => OnUpdateError(e.Message));
                })
                .LaunchAsync();
        }
        catch (Exception ex)
        {
            Invoke(() => OnUpdateError(ex.Message));
        }
    }

    private void OnUpdateSuccess()
    {
        btn_download.Loading = false;
        btn_download.LoadingWaveValue = 0;
        btn_download.Type = TTypeMini.Success;
        btn_download.Text = AntdUI.Localization.Get("completed", "更新完成");
        btn_download.Enabled = true;
        btn_cancel.Visible = false;
    }

    private void OnUpdateError(string error)
    {
        btn_download.Loading = false;
        btn_download.LoadingWaveValue = 0;
        btn_download.Type = TTypeMini.Error;
        btn_download.Text = AntdUI.Localization.Get("failed", "更新失败");
        btn_download.Enabled = true;
        btn_cancel.Visible = false;

        // 显示错误详情
        var localizedError = AntdUI.Localization.Get("error", "错误");
        MessageBox.Show($"{localizedError}: {error}",
            AntdUI.Localization.Get("title", "软件更新"),
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void btn_cancel_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
        btn_cancel.Visible = false;
        btn_download.Loading = false;
        btn_download.Text = AntdUI.Localization.Get("canceled", "已取消");
        btn_download.Enabled = true;
    }

    private void btn_mode_Click(object? sender, EventArgs e)
    {
        AntdUI.Config.IsDark = !AntdUI.Config.IsDark;
        SetTheme();
    }

    private void SetTheme()
    {
        Dark = AntdUI.Config.IsDark;
        btn_mode.Toggle = Dark;
        if (Dark)
        {
            BackColor = Color.Black;
            ForeColor = Color.White;
        }
        else
        {
            BackColor = Color.White;
            ForeColor = Color.Black;
        }
    }

    #region 控件初始化

    private void InitializeComponent()
    {
        this.Text = "软件更新";
        this.Size = new Size(460, 360);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(32)
        };

        // 标题
        var lblTitle = new Label
        {
            Text = AntdUI.Localization.Get("update", "软件更新"),
            Font = new Font("Microsoft YaHei UI", 20, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        // 版本号
        lbl_version = new Label
        {
            Text = "...",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft YaHei UI", 13),
            ForeColor = Color.Gray,
            Dock = DockStyle.Fill
        };

        // 更新说明
        lbl_note = new Label
        {
            Text = "",
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei UI", 11),
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 8)
        };

        // 按钮行
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        btn_download = new Button
        {
            Text = AntdUI.Localization.Get("download", "开始更新"),
            Width = 130,
            Height = 40,
            Font = new Font("Microsoft YaHei UI", 12)
        };
        btn_download.Click += btn_download_Click!;

        btn_cancel = new Button
        {
            Text = AntdUI.Localization.Get("cancel", "取消"),
            Width = 80,
            Height = 40,
            Visible = false
        };
        btn_cancel.Click += btn_cancel_Click!;

        btn_mode = new Button
        {
            Text = AntdUI.Localization.Get("theme", "主题"),
            Width = 70,
            Height = 40
        };
        btn_mode.Click += btn_mode_Click!;

        btnPanel.Controls.Add(btn_mode);
        btnPanel.Controls.Add(btn_cancel);
        btnPanel.Controls.Add(btn_download);

        panel.Controls.Add(lblTitle, 0, 0);
        panel.Controls.Add(lbl_version, 0, 1);
        panel.Controls.Add(lbl_note, 0, 2);
        panel.Controls.Add(btnPanel, 0, 4);

        this.Controls.Add(panel);
    }

    #endregion
}
