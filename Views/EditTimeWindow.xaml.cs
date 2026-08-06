using LightsOut.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace LightsOut.Views
{
    public partial class EditTimeWindow : Wpf.Ui.Controls.FluentWindow
    {
        public ShutdownTime Time { get; private set; }
        public bool IsDeleted { get; private set; }

        public EditTimeWindow(ShutdownTime? time = null)
        {
            InitializeComponent();

            if (time == null)
            {
                TitleText.Text = Helpers.LocalizationManager.Instance["AddButton"] ?? "添加";
                Time = new ShutdownTime
                {
                    Hour = DateTime.Now.Hour,
                    Minute = DateTime.Now.Minute
                };
                BtnDelete.Visibility = Visibility.Collapsed;
            }
            else
            {
                TitleText.Text = Helpers.LocalizationManager.Instance["TrayShowMainWindow"] ?? "设置"; // Or some edit text
                Time = time;
                BtnDelete.Visibility = Visibility.Visible;
            }

            HourBox.Value = Time.Hour;
            MinuteBox.Value = Time.Minute;
            BtnMon.IsChecked = Time.Monday;
            BtnTue.IsChecked = Time.Tuesday;
            BtnWed.IsChecked = Time.Wednesday;
            BtnThu.IsChecked = Time.Thursday;
            BtnFri.IsChecked = Time.Friday;
            BtnSat.IsChecked = Time.Saturday;
            BtnSun.IsChecked = Time.Sunday;

            UpdateRepeatUi();
        }

        private void OnRepeatRowClick(object sender, RoutedEventArgs e)
        {
            var isExpanded = RepeatPickerPanel.Visibility == Visibility.Visible;
            RepeatPickerPanel.Visibility = isExpanded ? Visibility.Collapsed : Visibility.Visible;
            RepeatChevronIcon.Symbol = isExpanded
                ? Wpf.Ui.Controls.SymbolRegular.ChevronRight24
                : Wpf.Ui.Controls.SymbolRegular.ChevronDown24;
        }

        private void OnPresetClick(object sender, RoutedEventArgs e)
        {
            if (sender == BtnPresetOnce)
            {
                SetDays(false, false, false, false, false, false, false);
            }
            else if (sender == BtnPresetWorkdays)
            {
                SetDays(true, true, true, true, true, false, false);
            }
            else if (sender == BtnPresetEveryday)
            {
                SetDays(true, true, true, true, true, true, true);
            }

            UpdateRepeatUi();
        }

        private void OnDayButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateRepeatUi();
        }

        private void SetDays(bool mon, bool tue, bool wed, bool thu, bool fri, bool sat, bool sun)
        {
            BtnMon.IsChecked = mon;
            BtnTue.IsChecked = tue;
            BtnWed.IsChecked = wed;
            BtnThu.IsChecked = thu;
            BtnFri.IsChecked = fri;
            BtnSat.IsChecked = sat;
            BtnSun.IsChecked = sun;
        }

        private void UpdateRepeatUi()
        {
            bool mon = BtnMon.IsChecked ?? false;
            bool tue = BtnTue.IsChecked ?? false;
            bool wed = BtnWed.IsChecked ?? false;
            bool thu = BtnThu.IsChecked ?? false;
            bool fri = BtnFri.IsChecked ?? false;
            bool sat = BtnSat.IsChecked ?? false;
            bool sun = BtnSun.IsChecked ?? false;

            RepeatSummaryText.Text = BuildRepeatSummary(mon, tue, wed, thu, fri, sat, sun);

            bool any = mon || tue || wed || thu || fri || sat || sun;
            BtnPresetOnce.IsChecked = !any;
            BtnPresetWorkdays.IsChecked = any && mon && tue && wed && thu && fri && !sat && !sun;
            BtnPresetEveryday.IsChecked = any && mon && tue && wed && thu && fri && sat && sun;
        }

        private static string BuildRepeatSummary(bool mon, bool tue, bool wed, bool thu, bool fri, bool sat, bool sun)
        {
            var lm = Helpers.LocalizationManager.Instance;
            bool any = mon || tue || wed || thu || fri || sat || sun;

            if (!any) return lm["RepeatOnce"];
            if (mon && tue && wed && thu && fri && sat && sun) return lm["RepeatEveryday"];
            if (mon && tue && wed && thu && fri && !sat && !sun) return lm["RepeatWorkdays"];

            var days = new List<string>();
            if (mon) days.Add(lm["Day1"]);
            if (tue) days.Add(lm["Day2"]);
            if (wed) days.Add(lm["Day3"]);
            if (thu) days.Add(lm["Day4"]);
            if (fri) days.Add(lm["Day5"]);
            if (sat) days.Add(lm["Day6"]);
            if (sun) days.Add(lm["Day7"]);
            return string.Join(", ", days);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Time.Hour = (int)(HourBox.Value ?? 0);
            Time.Minute = (int)(MinuteBox.Value ?? 0);
            Time.Monday = BtnMon.IsChecked ?? false;
            Time.Tuesday = BtnTue.IsChecked ?? false;
            Time.Wednesday = BtnWed.IsChecked ?? false;
            Time.Thursday = BtnThu.IsChecked ?? false;
            Time.Friday = BtnFri.IsChecked ?? false;
            Time.Saturday = BtnSat.IsChecked ?? false;
            Time.Sunday = BtnSun.IsChecked ?? false;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            IsDeleted = true;
            DialogResult = true;
            Close();
        }
    }
}
