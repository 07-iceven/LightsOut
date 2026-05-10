using System.Windows;
using LightsOut.Helpers;

namespace LightsOut;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        LocalizationManager.Initialize(SettingsService.Load().Language);
    }
}

