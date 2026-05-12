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
        var startupSettings = (Application.Current as App)?.StartupSettings ?? SettingsService.Load();
        ViewModel = new MainViewModel(startupSettings);
        DataContext = ViewModel;
        InitializeComponent();
        InitializeTrayIcon();
        LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        RefreshLocalizedUi();

        WeakReferenceMessenger.Default.Register<ShutdownWarningMessage>(this, OnShutdownWarningReceived);
        Debug.WriteLine("[LightsOut] MainWindow 已启动并注册消息监听");

        if (Environment.GetCommandLineArgs().Contains("--minimized"))
        {
            Hide();
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        RefreshLocalizedUi();
    }

    private void OnShutdownWarningReceived(object recipient, ShutdownWarningMessage message)
    {
        Debug.WriteLine("[LightsOut] MainWindow 接收到关机预警消息");
        Dispatcher.BeginInvoke(new Action(ShowAbortDialog));
    }

    private void InitializeTrayIcon()
    {
        _taskbarIcon = new TaskbarIcon();
        _taskbarIcon.Icon = System.Drawing.SystemIcons.Application;
        _taskbarIcon.TrayMouseDoubleClick += OnTrayMouseDoubleClick;

        var contextMenu = new ContextMenu();
        _showItem = new MenuItem();
        _showItem.Click += OnShowMenuItemClick;
        
        _exitItem = new MenuItem();
        _exitItem.Click += OnExitMenuItemClick;

        contextMenu.Items.Add(_showItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(_exitItem);
        _taskbarIcon.ContextMenu = contextMenu;
    }

    private void OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        ShowWindow();
    }

    private void OnShowMenuItemClick(object sender, RoutedEventArgs e)
    {
        ShowWindow();
    }

    private void OnExitMenuItemClick(object sender, RoutedEventArgs e)
    {
        _isExplicitExit = true;
        Application.Current.Shutdown();
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
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitExit)
        {
            e.Cancel = true;
            Hide();
            _taskbarIcon?.ShowBalloonTip(
                LocalizationManager.Instance["TrayMinimizedTitle"],
                LocalizationManager.Instance["TrayMinimizedMessage"],
                BalloonIcon.Info);
        }
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;

        if (_showItem != null)
        {
            _showItem.Click -= OnShowMenuItemClick;
        }

        if (_exitItem != null)
        {
            _exitItem.Click -= OnExitMenuItemClick;
        }

        if (_taskbarIcon != null)
        {
            _taskbarIcon.TrayMouseDoubleClick -= OnTrayMouseDoubleClick;
            _taskbarIcon.Dispose();
            _taskbarIcon = null;
        }

        ViewModel.Dispose();
        base.OnClosed(e);
    }

    private void ShowAbortDialog()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "-s -f -t 60",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            Debug.WriteLine("[LightsOut] 已下达 Windows 系统级关机指令 (-s -f -t 60)");

            var abortWin = new AbortWindow
            {
                Owner = this
            };
            abortWin.ShowDialog();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LightsOut] 触发关机流程失败: {ex}");
        }
    }
}
