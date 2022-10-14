using System.Windows;
using System.Windows.Controls;
using AutoOpticsWpf.Utils;
using AutoOpticsWpf.ViewModels;
using log4net;

namespace AutoOpticsWpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly HomePage _homePage = new();
        private readonly SerialPortSettingPage _serialPortSettingPage = new ();
        private readonly ColorMeterSettingPage _colorMeterSettingPage = new ();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
            
            Box.SelectedIndex = 0;

            ILog log = LogManager.GetLogger("CurrentClassName");
            log.Error("heheh");
        }
        
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Box.SelectedIndex)
            {
                case 0:
                    ContentFrame.Content = _homePage;
                    break;
                case 1:
                    ContentFrame.Content = _serialPortSettingPage;
                    break;
                case 2:
                    ContentFrame.Content = _colorMeterSettingPage;
                    break;
                default:
                    ContentFrame.Content = _homePage;
                    break;
            }
        }
    }
}
