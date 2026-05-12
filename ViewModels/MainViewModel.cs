using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightsOut.Helpers;
using LightsOut.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace LightsOut.ViewModels
{
    public class ShutdownWarningMessage { }

    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _saveTimer;
        private DateTime? _nextShutdownDateTime;
        private bool _isLoadingSettings;
        private bool _isDisposed;
        private bool _isActive;
        private string _countdownText = string.Empty;
        private bool _isStartupEnabled;
        private int _newHour = DateTime.Now.Hour;
        private int _newMinute = DateTime.Now.Minute;
        private string _selectedLanguage = LocalizationManager.Instance.CurrentLanguageCode;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    HandleIsActiveChanged(value);
                }
            }
        }

        public string CountdownText
        {
            get => _countdownText;
            private set => SetProperty(ref _countdownText, value);
        }

        public bool IsStartupEnabled
        {
            get => _isStartupEnabled;
            set
            {
                if (SetProperty(ref _isStartupEnabled, value))
                {
                    HandleStartupEnabledChanged(value);
                }
            }
        }

        public ObservableCollection<ShutdownTime> ShutdownTimes { get; } = new();

        public int NewHour
        {
            get => _newHour;
            set => SetProperty(ref _newHour, value);
        }

        public int NewMinute
        {
            get => _newMinute;
            set => SetProperty(ref _newMinute, value);
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    HandleSelectedLanguageChanged(value);
                }
            }
        }

        public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", DisplayName = "简体中文" },
            new LanguageOption { Code = "ja", DisplayName = "日本語" },
            new LanguageOption { Code = "en", DisplayName = "English" }
        };

        public IRelayCommand AddTimeCommand { get; }

        public IRelayCommand<ShutdownTime?> RemoveTimeCommand { get; }

        public MainViewModel(AppSettings? initialSettings = null)
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;

            _saveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _saveTimer.Tick += OnSaveTimerTick;

            AddTimeCommand = new RelayCommand(AddTime);
            RemoveTimeCommand = new RelayCommand<ShutdownTime?>(RemoveTime);

            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
            ShutdownTimes.CollectionChanged += OnShutdownTimesCollectionChanged;

            LoadSettings(initialSettings ?? SettingsService.Load());
            CheckStartupStatus();
            UpdateNextShutdownTime();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void OnSaveTimerTick(object? sender, EventArgs e)
        {
            _saveTimer.Stop();
            SaveSettingsCore();
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void OnShutdownTimesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ShutdownTime item in e.OldItems)
                    item.PropertyChanged -= OnShutdownTimePropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (ShutdownTime item in e.NewItems)
                    item.PropertyChanged += OnShutdownTimePropertyChanged;
            }
            UpdateNextShutdownTime();
            QueueSettingsSave();
        }

        private void OnShutdownTimePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShutdownTime.IsEnabled) ||
                e.PropertyName == nameof(ShutdownTime.Hour) ||
                e.PropertyName == nameof(ShutdownTime.Minute))
            {
                UpdateNextShutdownTime();
            }
            QueueSettingsSave();
        }

        private void LoadSettings(AppSettings settings)
        {
            _isLoadingSettings = true;

            IsActive = settings.IsActive;
            SetStartupEnabledSilently(settings.IsStartupEnabled);
            SelectedLanguage = string.IsNullOrWhiteSpace(settings.Language)
                ? LocalizationManager.Instance.CurrentLanguageCode
                : settings.Language;

            ShutdownTimes.Clear();
            foreach (var time in settings.ShutdownTimes)
            {
                ShutdownTimes.Add(time);
            }

            if (IsActive)
            {
                UpdateNextShutdownTime();
                _timer.Start();
            }

            _isLoadingSettings = false;
            UpdateCountdown();
        }

        private void QueueSettingsSave()
        {
            if (_isLoadingSettings || _isDisposed)
            {
                return;
            }

            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveSettingsCore()
        {
            if (_isLoadingSettings || _isDisposed)
            {
                return;
            }

            SettingsService.Save(CreateSettingsSnapshot());
        }

        private AppSettings CreateSettingsSnapshot()
        {
            return new AppSettings
            {
                IsActive = IsActive,
                ShutdownTimes = ShutdownTimes.Select(CloneShutdownTime).ToList(),
                IsStartupEnabled = IsStartupEnabled,
                Language = SelectedLanguage
            };
        }

        private static ShutdownTime CloneShutdownTime(ShutdownTime time)
        {
            return new ShutdownTime
            {
                Id = time.Id,
                Hour = time.Hour,
                Minute = time.Minute,
                IsEnabled = time.IsEnabled
            };
        }

        private void HandleIsActiveChanged(bool value)
        {
            if (value)
            {
                UpdateNextShutdownTime();
                _timer.Start();
                Debug.WriteLine($"[LightsOut] 任务已开启");
            }
            else
            {
                _timer.Stop();
                CancelSystemShutdown();
                CountdownText = LocalizationManager.Instance["StatusInactive"];
                Debug.WriteLine("[LightsOut] 任务已手动关闭");
            }

            QueueSettingsSave();
        }

        private void HandleSelectedLanguageChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            LocalizationManager.Instance.SetLanguage(value);
            UpdateCountdown();
            QueueSettingsSave();
        }

        private void AddTime()
        {
            ShutdownTimes.Add(new ShutdownTime { Hour = NewHour, Minute = NewMinute });
        }

        private void RemoveTime(ShutdownTime? time)
        {
            if (time != null)
            {
                ShutdownTimes.Remove(time);
            }
        }

        public void UpdateNextShutdownTime()
        {
            if (!IsActive || !ShutdownTimes.Any(t => t.IsEnabled))
            {
                _nextShutdownDateTime = null;
                return;
            }

            var now = DateTime.Now;
            List<DateTime> candidates = [];

            foreach (var st in ShutdownTimes.Where(t => t.IsEnabled))
            {
                var target = now.Date.AddHours(st.Hour).AddMinutes(st.Minute);
                if (target <= now)
                {
                    target = target.AddDays(1);
                }
                candidates.Add(target);
            }

            if (candidates.Any())
            {
                _nextShutdownDateTime = candidates.Min();
                Debug.WriteLine($"[LightsOut] 下一个关机时间: {_nextShutdownDateTime}");
            }
            else
            {
                _nextShutdownDateTime = null;
            }
        }

        private void UpdateCountdown()
        {
            if (_nextShutdownDateTime == null)
            {
                UpdateNextShutdownTime();
                if (_nextShutdownDateTime == null)
                {
                    CountdownText = IsActive
                        ? LocalizationManager.Instance["StatusNoEnabledTimes"]
                        : LocalizationManager.Instance["StatusInactive"];
                    return;
                }
            }

            var now = DateTime.Now;
            var remaining = _nextShutdownDateTime.Value - now;

            if (remaining.TotalSeconds <= 0)
            {
                Debug.WriteLine("[LightsOut] !!! 触发关机预警 !!!");
                
                // 立即计算下一个时间点
                UpdateNextShutdownTime();

                WeakReferenceMessenger.Default.Send(new ShutdownWarningMessage());
                return;
            }

            CountdownText = LocalizationManager.Instance.Format(
                "CountdownFormat",
                remaining.Hours,
                remaining.Minutes,
                remaining.Seconds);
        }

        private void CancelSystemShutdown()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "-a",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LightsOut] 取消系统关机失败: {ex}");
            }
        }

        private void HandleStartupEnabledChanged(bool value)
        {
            if (SetStartup(value))
            {
                QueueSettingsSave();
                return;
            }

            CheckStartupStatus();
        }

        private void CheckStartupStatus()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                SetStartupEnabledSilently(key?.GetValue("LightsOut") != null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LightsOut] 检查开机自启状态失败: {ex}");
            }
        }

        private void SetStartupEnabledSilently(bool value)
        {
            SetProperty(ref _isStartupEnabled, value, nameof(IsStartupEnabled));
        }

        private bool SetStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    Debug.WriteLine("[LightsOut] 无法打开开机自启注册表项");
                    return false;
                }

                if (enable)
                {
                    string? path = Process.GetCurrentProcess().MainModule?.FileName;
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        Debug.WriteLine("[LightsOut] 无法获取当前程序路径，未写入开机自启");
                        return false;
                    }

                    key.SetValue("LightsOut", $"\"{path}\" --minimized");
                    return true;
                }

                key.DeleteValue("LightsOut", false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LightsOut] 设置开机自启失败: {ex}");
                return false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _timer.Stop();
            _saveTimer.Stop();
            SaveSettingsCore();
            _isDisposed = true;

            _timer.Tick -= OnTimerTick;
            _saveTimer.Tick -= OnSaveTimerTick;
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
            ShutdownTimes.CollectionChanged -= OnShutdownTimesCollectionChanged;

            foreach (var time in ShutdownTimes)
            {
                time.PropertyChanged -= OnShutdownTimePropertyChanged;
            }

            GC.SuppressFinalize(this);
        }
    }
}
