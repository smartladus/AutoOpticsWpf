using CommunityToolkit.Mvvm.ComponentModel;
using AutoOpticsWpf.Utils;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoOpticsWpf.ViewModels
{
    internal class MainViewModel: ObservableRecipient
    {
        private readonly SerialPortManager _spManager = SerialPortManager.GetInstance();
        private readonly CaDeviceManager _caDeviceManager = CaDeviceManager.GetInstance();


        private bool _isSerialPortOpened;
        private string _serialPortStat;

        private bool _isCaDeviceConnected;
        private string _caDeviceStat;

        public bool IsSerialPortOpened
        {
            get => _isSerialPortOpened;
            set => SetProperty(ref _isSerialPortOpened, value);
        }


        public string SerialPortStat
        {
            get => _serialPortStat;
            internal set => SetProperty(ref _serialPortStat, value);
        }

        public string CaDeviceStat
        {
            get => _caDeviceStat;
            internal set => SetProperty(ref _caDeviceStat, value);
        }

        public bool IsCaDeviceConnected
        {
            get => _isCaDeviceConnected;
            set => SetProperty(ref _isCaDeviceConnected, value);
        }

        public MainViewModel()
        {
            UpdateSerialPortStat();
            UpdateCaDeviceStat();

            WeakReferenceMessenger.Default.Register<SerialPortStatusChangedMessage>(this, (r, m) =>
            {
                UpdateSerialPortStat();
            });
            WeakReferenceMessenger.Default.Register<CaStatusChangedMessage>(this, (r, m) =>
            {
                UpdateCaDeviceStat();
            });
        }

        private void UpdateSerialPortStat()
        {
            IsSerialPortOpened = _spManager.IsPortOpen();
            SerialPortStat = _spManager.GetSerialPortStatStr(true);
        }

        private void UpdateCaDeviceStat()
        {
            IsCaDeviceConnected = _caDeviceManager.IsDevConnected();
            CaDeviceStat = _caDeviceManager.GetCaDeviceStatStr(true);
        }
    }
}
