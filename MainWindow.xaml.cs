using System.Windows;
using LightsOut.ViewModels;
using LightsOut.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System;
using System.ComponentModel;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Linq;
using LightsOut.Helpers;

namespace LightsOut;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private TaskbarIcon? _taskbarIcon;
    private bool _isExplicitExit = false;
    private MenuItem? _showItem;
    private MenuItem? _exitItem;

    public MainWindow()
    {
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        InitializeComponent();
        InitializeTrayIcon();
        LocalizationManager.Instance.LanguageChanged += (_, _) => RefreshLocalizedUi();
        RefreshLocalizedUi();

        // 注册消息接收
        WeakReferenceMessenger.Default.Register<ShutdownWarningMessage>(this, (r, m) =>
        {
            Debug.WriteLine("[LightsOut] MainWindow 接收到关机预警消息");
            Dispatcher.BeginInvoke(new Action(() => ShowAbortDialog()));
        });
        
        Debug.WriteLine("[LightsOut] MainWindow 已启动并注册消息监听");

        // 检查启动参数
        if (Environment.GetCommandLineArgs().Contains("--minimized"))
        {
            this.Hide();
        }
    }

    private void InitializeTrayIcon()
    {
        _taskbarIcon = new TaskbarIcon();
        // 设置托盘图标（这里先使用系统默认图标，实际开发建议准备一个ico资源）
        _taskbarIcon.Icon = System.Drawing.SystemIcons.Application;
        
        // 双击托盘图标显示窗口
        _taskbarIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();

        // 右键菜单
        var contextMenu = new ContextMenu();
        _showItem = new MenuItem();
        _showItem.Click += (s, e) => ShowWindow();
        
        _exitItem = new MenuItem();
        _exitItem.Click += (s, e) => 
        {
            _isExplicitExit = true;
            Application.Current.Shutdown();
        };

        contextMenu.Items.Add(_showItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(_exitItem);
        _taskbarIcon.ContextMenu = contextMenu;
    }

    private void RefreshLocalizedUi()
    {
        if (_taskbarIcon != null)
        {
            _taskbarIcon.ToolTipText = LocalizationManager.Instance["TrayToolTip"];
        }

        if (_showItem != null)
        {
            _showItem.Header = LocalizationManager.Instance["TrayShowMainWindow"];
        }

        if (_exitItem != null)
        {
            _exitItem.Header = LocalizationManager.Instance["TrayExit"];
        }
    }

    private void ShowWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitExit)
        {
            e.Cancel = true;
            this.Hide(); // 隐藏窗口而非退出
            _taskbarIcon?.ShowBalloonTip(
                LocalizationManager.Instance["TrayMinimizedTitle"],
                LocalizationManager.Instance["TrayMinimizedMessage"],
                BalloonIcon.Info);
        }
        base.OnClosing(e);
    }

    private void ShowAbortDialog()
    {
        try 
        {
            // 1. 先下达 Windows 系统级关机指令 (60秒预警)
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "-s -f -t 60", 
                CreateNoWindow = true,
                UseShellExecute = false
            });
            Debug.WriteLine("[LightsOut] 已下达 Windows 系统级关机指令 (-s -f -t 60)");

            // 2. 弹出“取消关机”按钮窗口
            var abortWin = new AbortWindow(ViewModel);
            abortWin.Owner = this;
            abortWin.ShowDialog();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LightsOut] 触发关机流程失败: {ex.Message}");
        }
    }
}
