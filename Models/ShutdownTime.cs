using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace LightsOut.Models
{
    public class ShutdownTime : ObservableObject
    {
        private int _hour;
        private int _minute;
        private bool _isEnabled = true;
        private bool _monday;
        private bool _tuesday;
        private bool _wednesday;
        private bool _thursday;
        private bool _friday;
        private bool _saturday;
        private bool _sunday;

        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonPropertyName("hour")]
        public int Hour
        {
            get => _hour;
            set
            {
                if (SetProperty(ref _hour, value))
                {
                    OnPropertyChanged(nameof(DisplayTime));
                }
            }
        }

        [JsonPropertyName("minute")]
        public int Minute
        {
            get => _minute;
            set
            {
                if (SetProperty(ref _minute, value))
                {
                    OnPropertyChanged(nameof(DisplayTime));
                }
            }
        }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        [JsonIgnore]
        public bool IsRepeat => Monday || Tuesday || Wednesday || Thursday || Friday || Saturday || Sunday;

        [JsonPropertyName("monday")]
        public bool Monday
        {
            get => _monday;
            set
            {
                if (SetProperty(ref _monday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonPropertyName("tuesday")]
        public bool Tuesday
        {
            get => _tuesday;
            set
            {
                if (SetProperty(ref _tuesday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonPropertyName("wednesday")]
        public bool Wednesday
        {
            get => _wednesday;
            set
            {
                if (SetProperty(ref _wednesday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonPropertyName("thursday")]
        public bool Thursday
        {
            get => _thursday;
            set
            {
                if (SetProperty(ref _thursday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonPropertyName("friday")]
        public bool Friday
        {
            get => _friday;
            set
            {
                if (SetProperty(ref _friday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonPropertyName("saturday")]
        public bool Saturday
        {
            get => _saturday;
            set
            {
                if (SetProperty(ref _saturday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonPropertyName("sunday")]
        public bool Sunday
        {
            get => _sunday;
            set
            {
                if (SetProperty(ref _sunday, value))
                    OnPropertyChanged(nameof(RepeatText));
            }
        }

        [JsonIgnore]
        public string DisplayTime => $"{Hour:D2}:{Minute:D2}";

        [JsonIgnore]
        public string RepeatText
        {
            get
            {
                if (!IsRepeat) return Helpers.LocalizationManager.Instance["RepeatOnce"] ?? "只响一次";
                
                if (Monday && Tuesday && Wednesday && Thursday && Friday && Saturday && Sunday)
                    return Helpers.LocalizationManager.Instance["RepeatEveryday"] ?? "每天";
                    
                if (Monday && Tuesday && Wednesday && Thursday && Friday && !Saturday && !Sunday)
                    return Helpers.LocalizationManager.Instance["RepeatWorkdays"] ?? "周一至周五";
                    
                var days = new System.Collections.Generic.List<string>();
                if (Monday) days.Add(Helpers.LocalizationManager.Instance["Day1"] ?? "周一");
                if (Tuesday) days.Add(Helpers.LocalizationManager.Instance["Day2"] ?? "周二");
                if (Wednesday) days.Add(Helpers.LocalizationManager.Instance["Day3"] ?? "周三");
                if (Thursday) days.Add(Helpers.LocalizationManager.Instance["Day4"] ?? "周四");
                if (Friday) days.Add(Helpers.LocalizationManager.Instance["Day5"] ?? "周五");
                if (Saturday) days.Add(Helpers.LocalizationManager.Instance["Day6"] ?? "周六");
                if (Sunday) days.Add(Helpers.LocalizationManager.Instance["Day7"] ?? "周日");
                
                if (days.Count == 0) return Helpers.LocalizationManager.Instance["RepeatNotSet"] ?? "未设置";
                return string.Join(", ", days);
            }
        }
    }
}
