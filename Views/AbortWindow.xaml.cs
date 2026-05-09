using System;
using System.Diagnostics;
using System.Windows;
using LightsOut.ViewModels;

namespace LightsOut.Views
{
    public partial class AbortWindow : Window
    {
        private MainViewModel _viewModel;

        public AbortWindow(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 执行取消关机命令
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "-a",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                
                // 注意：这里不再设置 _viewModel.IsActive = false
                // 主开关将保持开启，以便下一个时间点能正常触发
                
                Debug.WriteLine("[LightsOut] 用户点击了取消按钮，已发送 shutdown -a，主开关保持开启状态");
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LightsOut] 取消关机失败: {ex.Message}");
                this.Close();
            }
        }
    }
}
