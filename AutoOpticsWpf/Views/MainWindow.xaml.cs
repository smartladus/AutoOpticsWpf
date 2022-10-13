using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AutoOpticsWpf.Utils;
using log4net;

namespace AutoOpticsWpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SerialPortManager spManager = SerialPortManager.GetInstance();
        CaDeviceManager caDeviceManager = CaDeviceManager.GetInstance();
        AutoWhiteBalanceManager awbManager = AutoWhiteBalanceManager.GetInstance();

        public MainWindow()
        {
            InitializeComponent();
            spManager.ComDataReceived += OnDataReceived;
            caDeviceManager.SimpMeasDataReceived += OnOpticsDataReceived;

            ContentFrame.Content = new SerialPortSettingPage();

            ILog log = LogManager.GetLogger("CurrentClassName");
            log.Error("heheh");
        }


        private void ConnectSerialPort(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = new SerialPortSettingPage();
        }

        private void SendStringData(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = new ColorMeterSettingPage();
        }

        private void SendHexData(object sender, RoutedEventArgs e)
        {
            spManager.SendData(SerialPortManager.HexStr2ByteArr("D5 11 11 36 00 AA"));
        }

        private void OnDataReceived(object sender, ComDataReceivedEventArgs e)
        {
            byte[] data = e.DataBytes;
            Trace.WriteLine("外面收到了: ");
            Trace.WriteLine("ASCII: " + SerialPortManager.ByteArr2AscStr(data));
            Trace.WriteLine("HEX  : " + SerialPortManager.ByteArr2HexStr(data));
        }


        private void ConnectCaDevice(object sender, RoutedEventArgs e)
        {
            caDeviceManager.Connect()
                .UseDefaultSetting();
        }

        private void OnOpticsDataReceived(object sender, SimpMeasDataReceivedEventArgs data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Optics Data Received, Lv: " + data.Lv)
                .Append(", u': " + data.Ud)
                .Append(", v': " + data.Vd)
                .Append(", x: " + data.Sx)
                .Append(", y: " + data.Sy);
            Trace.WriteLine(sb);
        }

        private void GetInfo(object sender, RoutedEventArgs e)
        {
            awbManager.PerformAutoWhiteBalance(10);
        }

        private void GetOpticsData(object sender, RoutedEventArgs e)
        {
            caDeviceManager.SimpleMeasure();
        }
    }
}
