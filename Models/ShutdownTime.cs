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
        public string DisplayTime => $"{Hour:D2}:{Minute:D2}";
    }
}
