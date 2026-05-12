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
}

