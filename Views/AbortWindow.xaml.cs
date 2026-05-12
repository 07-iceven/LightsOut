using System;
using System.Diagnostics;
using System.Windows;

namespace LightsOut.Views
{
    public partial class AbortWindow : Window
    {
        public AbortWindow()
        {
            InitializeComponent();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
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

                Debug.WriteLine("[LightsOut] 用户点击了取消按钮，已发送 shutdown -a，主开关保持开启状态");
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LightsOut] 取消关机失败: {ex}");
                Close();
            }
        }
    }
}
