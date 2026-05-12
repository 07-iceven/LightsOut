using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightsOut.Models;

namespace LightsOut.Helpers
{
    public class AppSettings
    {
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("shutdown_times")]
        public List<ShutdownTime> ShutdownTimes { get; set; } = new();

        [JsonPropertyName("is_startup_enabled")]
        public bool IsStartupEnabled { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }

    public static class SettingsService
    {
        private static readonly object SyncRoot = new();
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "LightsOut");

        private static readonly string SettingsPath = Path.Combine(AppDataPath, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static AppSettings? _cachedSettings;

        public static AppSettings Load()
        {
            lock (SyncRoot)
            {
                if (_cachedSettings != null)
                {
                    return Clone(_cachedSettings);
                }

                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        string json = File.ReadAllText(SettingsPath);
                        _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                        return Clone(_cachedSettings);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex}");
                }

                _cachedSettings = new AppSettings();
                return Clone(_cachedSettings);
            }
        }

        public static void Save(AppSettings settings)
        {
            lock (SyncRoot)
            {
                try
                {
                    if (!Directory.Exists(AppDataPath))
                    {
                        Directory.CreateDirectory(AppDataPath);
                    }

                    var snapshot = Clone(settings);
                    string json = JsonSerializer.Serialize(snapshot, JsonOptions);
                    File.WriteAllText(SettingsPath, json);
                    _cachedSettings = snapshot;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex}");
                }
            }
        }

        private static AppSettings Clone(AppSettings settings)
        {
            return new AppSettings
            {
                IsActive = settings.IsActive,
                IsStartupEnabled = settings.IsStartupEnabled,
                Language = settings.Language,
                ShutdownTimes = settings.ShutdownTimes
                    .Select(time => new ShutdownTime
                    {
                        Id = time.Id,
                        Hour = time.Hour,
                        Minute = time.Minute,
                        IsEnabled = time.IsEnabled
                    })
                    .ToList()
            };
        }
    }
}
