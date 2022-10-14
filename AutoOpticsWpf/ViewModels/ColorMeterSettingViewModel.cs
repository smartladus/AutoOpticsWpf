using AutoOpticsWpf.Utils;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AutoOpticsWpf.ViewModels
{
    internal class ColorMeterSettingViewModel : ObservableRecipient
    {
        private readonly CaDeviceManager _caDeviceManager = CaDeviceManager.GetInstance();

        private bool _useMannualSetting;

        private bool UseMannualSetting
        {
            get => _useMannualSetting;
            set => SetProperty(ref _useMannualSetting, value);
        }


        public ICommand ConnectCaDeviceCommand { get; }
        public ICommand GetOpticsDataCommand { get; }

        public ColorMeterSettingViewModel()
        {
            UseMannualSetting = false;

            ConnectCaDeviceCommand = new RelayCommand(ConnectCaDevice);
            GetOpticsDataCommand = new RelayCommand(GetOpticsData);
            _caDeviceManager.SimpMeasDataReceived += OnOpticsDataReceived;
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

        private void ConnectCaDevice()
        {
            _caDeviceManager.Connect()
                .UseDefaultSetting();
        }

        private void GetOpticsData()
        {
            _caDeviceManager.SimpleMeasure();
        }
    }
}
