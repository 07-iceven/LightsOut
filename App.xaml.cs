using System.Windows;
using LightsOut.Helpers;

namespace LightsOut;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public AppSettings StartupSettings { get; }

    public App()
    {
        StartupSettings = SettingsService.Load();
        LocalizationManager.Initialize(StartupSettings.Language);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyInitialTheme(StartupSettings.Theme);
    }

    private void ApplyInitialTheme(AppTheme theme)
    {
        var wpfuiTheme = theme switch
        {
            AppTheme.Light => Wpf.Ui.Appearance.ApplicationTheme.Light,
            AppTheme.Dark => Wpf.Ui.Appearance.ApplicationTheme.Dark,
            _ => Wpf.Ui.Appearance.ApplicationTheme.Unknown
        };

        if (theme == AppTheme.System)
        {
            Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();
        }
        else
        {
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(wpfuiTheme);
        }
    }
}

