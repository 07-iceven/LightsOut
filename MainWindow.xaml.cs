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

namespace LightsOut;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private TaskbarIcon? _taskbarIcon;
    private bool _isExplicitExit = false;

    public MainWindow()
    {
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        InitializeComponent();
        InitializeTrayIcon();

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
        _taskbarIcon.ToolTipText = "LightsOut - 自动关机工具";
        
        // 双击托盘图标显示窗口
        _taskbarIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();

        // 右键菜单
        var contextMenu = new ContextMenu();
        var showItem = new MenuItem { Header = "显示主界面" };
        showItem.Click += (s, e) => ShowWindow();
        
        var exitItem = new MenuItem { Header = "完全退出" };
        exitItem.Click += (s, e) => 
        {
            _isExplicitExit = true;
            Application.Current.Shutdown();
        };

        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);
        _taskbarIcon.ContextMenu = contextMenu;
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
            _taskbarIcon?.ShowBalloonTip("LightsOut", "程序已最小化到托盘，将继续运行计划任务", BalloonIcon.Info);
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
