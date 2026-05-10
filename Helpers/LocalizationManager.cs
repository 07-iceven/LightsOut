using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace LightsOut.Helpers;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    private static readonly ResourceManager ResourceManager =
        new("LightsOut.Resources.Strings", Assembly.GetExecutingAssembly());

    public static LocalizationManager Instance { get; } = new();

    private CultureInfo _currentCulture;

    private LocalizationManager()
    {
        _currentCulture = ResolveCulture(null);
        ApplyCulture(_currentCulture);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? LanguageChanged;

    public string CurrentLanguageCode => NormalizeLanguageCode(_currentCulture.Name);

    public string this[string key] => GetString(key);

    public static void Initialize(string? languageCode)
    {
        Instance.SetLanguage(languageCode);
    }

    public void SetLanguage(string? languageCode)
    {
        var culture = ResolveCulture(languageCode);
        if (_currentCulture.Name == culture.Name)
        {
            return;
        }

        _currentCulture = culture;
        ApplyCulture(culture);
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(CurrentLanguageCode));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key)
    {
        return ResourceManager.GetString(key, _currentCulture) ?? key;
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(_currentCulture, GetString(key), args);
    }

    private static void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    private static CultureInfo ResolveCulture(string? languageCode)
    {
        var normalized = NormalizeLanguageCode(languageCode);
        return new CultureInfo(normalized);
    }

    private static string NormalizeLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DetectLanguageCode(CultureInfo.CurrentUICulture);
        }

        if (languageCode.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-CN";
        }

        if (languageCode.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
        {
            return "ja";
        }

        return "en";
    }

    private static string DetectLanguageCode(CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName switch
        {
            "zh" => "zh-CN",
            "ja" => "ja",
            _ => "en"
        };
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
