using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoOpticsWpf.Utils;
using AutoOpticsWpf.ViewModels.UserControls;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoOpticsWpf.Views
{
    /// <summary>
    /// SerialPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class SerialPortSettingPage : Page
    {
        public SerialPortSettingPage()
        {
            InitializeComponent();
            DataContext = new SerialPortSettingViewModel();

            WeakReferenceMessenger.Default.Register<SerialPortStatusChangedMessage>(this, (r, m) =>
            {
                SetOpenButton(m.Value);
            });
        }

        private void SetOpenButton(bool isOpen)
        {
            BtnOpen.Content = isOpen ? "关闭串口" : "打开串口";
        }
    }
}
