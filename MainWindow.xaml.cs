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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace LightsOut;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private const double TimeListDragThreshold = 6;

    public MainViewModel ViewModel { get; }
    private TaskbarIcon? _taskbarIcon;
    private bool _isExplicitExit = false;
    private MenuItem? _showItem;
    private MenuItem? _exitItem;
    private ScrollViewer? _timeListScrollViewer;
    private bool _isTimeListDragPending;
    private bool _isTimeListDragging;
    private Point _timeListDragStartPoint;
    private double _timeListDragStartOffset;

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
        Debug.WriteLine("[Lights Out] MainWindow 已启动并注册消息监听");

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
        Debug.WriteLine("[Lights Out] MainWindow 接收到关机预警消息");
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
            Debug.WriteLine("[Lights Out] 已下达 Windows 系统级关机指令 (-s -f -t 60)");

            var abortWin = new AbortWindow
            {
                Owner = this
            };
            abortWin.ShowDialog();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Lights Out] 触发关机流程失败: {ex}");
        }
    }

    private void OnTimeListLoaded(object sender, RoutedEventArgs e)
    {
        _timeListScrollViewer = FindDescendant<ScrollViewer>(TimeListBox);
    }

    private void OnTimeListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_timeListScrollViewer == null
            || e.ChangedButton != MouseButton.Left
            || e.StylusDevice?.TabletDevice.Type == TabletDeviceType.Touch
            || IsInteractiveElement(e.OriginalSource))
        {
            return;
        }

        _isTimeListDragPending = true;
        _isTimeListDragging = false;
        _timeListDragStartPoint = e.GetPosition(this);
        _timeListDragStartOffset = _timeListScrollViewer.VerticalOffset;
    }

    private void OnTimeListPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_timeListScrollViewer == null || !_isTimeListDragPending || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPosition = e.GetPosition(this);
        var delta = currentPosition.Y - _timeListDragStartPoint.Y;

        if (!_isTimeListDragging)
        {
            if (Math.Abs(delta) < TimeListDragThreshold)
            {
                return;
            }

            _isTimeListDragging = true;
            TimeListBox.CaptureMouse();
            Mouse.OverrideCursor = Cursors.SizeNS;
        }

        _timeListScrollViewer.ScrollToVerticalOffset(_timeListDragStartOffset - delta);
        e.Handled = true;
    }

    private void OnTimeListPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        EndTimeListDrag();
    }

    private void OnTimeListLostMouseCapture(object sender, MouseEventArgs e)
    {
        EndTimeListDrag();
    }

    private void EndTimeListDrag()
    {
        if (_isTimeListDragging && TimeListBox.IsMouseCaptured)
        {
            TimeListBox.ReleaseMouseCapture();
        }

        _isTimeListDragPending = false;
        _isTimeListDragging = false;

        if (Mouse.OverrideCursor == Cursors.SizeNS)
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static bool IsInteractiveElement(object originalSource)
    {
        if (originalSource is not DependencyObject dependencyObject)
        {
            return false;
        }

        return FindAncestor<ButtonBase>(dependencyObject) != null
            || FindAncestor<ToggleButton>(dependencyObject) != null
            || FindAncestor<TextBoxBase>(dependencyObject) != null
            || FindAncestor<RepeatButton>(dependencyObject) != null
            || FindAncestor<ScrollBar>(dependencyObject) != null;
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T found)
            {
                return found;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T ancestor)
            {
                return ancestor;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
