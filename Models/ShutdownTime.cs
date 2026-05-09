using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace LightsOut.Models
{
    public partial class ShutdownTime : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        [property: JsonPropertyName("hour")]
        private int _hour;

        [ObservableProperty]
        [property: JsonPropertyName("minute")]
        private int _minute;

        [ObservableProperty]
        [property: JsonPropertyName("is_enabled")]
        private bool _isEnabled = true;

        [JsonIgnore]
        public string DisplayTime => $"{Hour:D2}:{Minute:D2}";

        // 当属性改变时，DisplayTime 也需要更新
        partial void OnHourChanged(int value) => OnPropertyChanged(nameof(DisplayTime));
        partial void OnMinuteChanged(int value) => OnPropertyChanged(nameof(DisplayTime));
    }
}
