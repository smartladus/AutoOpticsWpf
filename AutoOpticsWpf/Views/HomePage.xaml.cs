using System.Windows.Controls;
using AutoOpticsWpf.ViewModels;

namespace AutoOpticsWpf.Views
{
    /// <summary>
    /// SerialPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomeViewModel();

        }
    }
}
