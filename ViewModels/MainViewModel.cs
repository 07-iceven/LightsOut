using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Microsoft.Win32;
using System.Windows;
using System.Diagnostics;
using LightsOut.Models;
using LightsOut.Helpers;

namespace LightsOut.ViewModels
{
    public class ShutdownWarningMessage { }

    public partial class MainViewModel : ObservableObject
    {
        private System.Timers.Timer _timer;
        private DateTime? _nextShutdownDateTime;
        private bool _isLoadingSettings;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private string _countdownText = string.Empty;

        [ObservableProperty]
        private bool _isStartupEnabled;

        [ObservableProperty]
        private ObservableCollection<ShutdownTime> _shutdownTimes = new();

        [ObservableProperty]
        private int _newHour = DateTime.Now.Hour;

        [ObservableProperty]
        private int _newMinute = DateTime.Now.Minute;

        [ObservableProperty]
        private string _selectedLanguage = LocalizationManager.Instance.CurrentLanguageCode;

        public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", DisplayName = "简体中文" },
            new LanguageOption { Code = "ja", DisplayName = "日本語" },
            new LanguageOption { Code = "en", DisplayName = "English" }
        };

        public MainViewModel()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;

            LocalizationManager.Instance.LanguageChanged += (_, _) =>
            {
                UpdateCountdown();
            };

            ShutdownTimes.CollectionChanged += OnShutdownTimesCollectionChanged;

            LoadSettings();
            CheckStartupStatus();

            // 初始计算一次，确保倒计时正确
            UpdateNextShutdownTime();
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
            SaveSettings();
        }

        private void OnShutdownTimePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShutdownTime.IsEnabled) || 
                e.PropertyName == nameof(ShutdownTime.Hour) || 
                e.PropertyName == nameof(ShutdownTime.Minute))
            {
                UpdateNextShutdownTime();
            }
            SaveSettings();
        }

        private void LoadSettings()
        {
            _isLoadingSettings = true;
            var settings = SettingsService.Load();
            IsActive = settings.IsActive;
            SelectedLanguage = string.IsNullOrWhiteSpace(settings.Language)
                ? LocalizationManager.Instance.CurrentLanguageCode
                : settings.Language;
            
            // 清空并重新填充，以触发 CollectionChanged
            ShutdownTimes.Clear();
            foreach (var time in settings.ShutdownTimes)
            {
                ShutdownTimes.Add(time);
            }

            if (ShutdownTimes.Count == 0)
            {
                ShutdownTimes.Add(new ShutdownTime { Hour = 23, Minute = 0 });
            }
            
            if (IsActive)
            {
                UpdateNextShutdownTime();
                _timer.Start();
            }

            _isLoadingSettings = false;
            UpdateCountdown();
        }

        private void SaveSettings()
        {
            SettingsService.Save(new AppSettings
            {
                IsActive = IsActive,
                ShutdownTimes = ShutdownTimes.ToList(),
                IsStartupEnabled = IsStartupEnabled,
                Language = SelectedLanguage
            });
        }

        partial void OnIsActiveChanged(bool value)
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
            SaveSettings();
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            LocalizationManager.Instance.SetLanguage(value);
            UpdateCountdown();

            if (!_isLoadingSettings)
            {
                SaveSettings();
            }
        }

        [RelayCommand]
        private void AddTime()
        {
            ShutdownTimes.Add(new ShutdownTime { Hour = NewHour, Minute = NewMinute });
        }

        [RelayCommand]
        private void RemoveTime(ShutdownTime time)
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
            var candidates = new List<DateTime>();

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

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            UpdateCountdown();
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

                Application.Current.Dispatcher.BeginInvoke(new Action(() => 
                {
                    WeakReferenceMessenger.Default.Send(new ShutdownWarningMessage());
                }));
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
            catch { }
        }

        partial void OnIsStartupEnabledChanged(bool value)
        {
            SetStartup(value);
            SaveSettings();
        }

        private void CheckStartupStatus()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                IsStartupEnabled = key?.GetValue("LightsOut") != null;
            }
            catch { }
        }

        private void SetStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (enable)
                {
                    string? path = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (path != null)
                    {
                        // 添加 --minimized 参数，以便自启动时隐藏到托盘
                        key?.SetValue("LightsOut", $"\"{path}\" --minimized");
                    }
                }
                else
                {
                    key?.DeleteValue("LightsOut", false);
                }
            }
            catch { }
        }
    }
}
