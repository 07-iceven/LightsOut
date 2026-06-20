using LightsOut.Models;
using System;
using System.Windows;

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