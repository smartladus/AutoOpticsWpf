using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AutoOpticsWpf.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoOpticsWpf.ViewModels.UserControls
{
    internal class SerialPortSettingViewModel : ObservableRecipient
    {

        private bool _isPortOpened;

        public bool IsPortOpened
        {
            get => _isPortOpened;
            internal set => SetProperty(ref _isPortOpened, value);
        }

        public string[] PortNameArr { get; internal set; }
        public int[] BaudRateArr { get; internal set; }
        public int[] DataBitsArr { get; internal set; }
        public Parity[] ParityArr { get; internal set; }
        public StopBits[] StopBitsArr { get; internal set; }

        private SerialPortManager spManager = SerialPortManager.GetInstance();

        private string _portName;
        private int _baudRate;
        private int _dataBits;
        private Parity _parity;
        private StopBits _stopBits;

        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }
        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }
        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }
        public Parity Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }
        public StopBits StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private bool _rxInHex;
        private bool _rxAutoWrap;
        private bool _rxShowDate;
        private string _tbRxStr;


        private bool _txInHex;
        private string _tbTxStr;


        public bool RxInHex
        {
            get => _rxInHex;
            set => SetProperty(ref _rxInHex, value);
        }

        public bool RxAutoWrap
        {
            get => _rxAutoWrap;
            set => SetProperty(ref _rxAutoWrap, value);
        }
        public bool RxShowDate
        {
            get => _rxShowDate;
            set => SetProperty(ref _rxShowDate, value);
        }

        public string TbRxStr
        {
            get => _tbRxStr;
            set => SetProperty(ref _tbRxStr, value);
        }

        public bool TxInHex
        {
            get => _txInHex;
            set => SetProperty(ref _txInHex, value);
        }

        public string TbTxStr
        {
            get => _tbTxStr;
            set => SetProperty(ref _tbTxStr, value);
        }

        public SerialPortSettingViewModel()
        {
            IsPortOpened = spManager.IsPortOpen();

            PortNameArr = spManager.GetSerialPortNameArr();
            PortName = PortNameArr.Length > 0 ? PortNameArr[0] : "";
            BaudRateArr = new[] { 9600, 19200, 38400, 57600, 115200 };
            DataBitsArr = new[] { 5, 6, 7, 8 };
            ParityArr = new [] { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
            StopBitsArr = new[] { StopBits.One, StopBits.OnePointFive, StopBits.Two, StopBits.None };
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;

            RxInHex = false;
            RxAutoWrap = true;
            RxShowDate = false;
            TbRxStr = "";

            TxInHex = false;
            TbTxStr = "";

            ConnectSerialPortCommand = new RelayCommand(ConnectSerialPort);
            SendMsgCommand = new RelayCommand(SendMsg);

            spManager.ComDataReceived += OnComDataReceived;

            WeakReferenceMessenger.Default.Register<SerialPortStatusChangedMessage>(this, (r, m) =>
            {
                IsPortOpened = m.Value;
            });
        }

        public ICommand ConnectSerialPortCommand { get; }

        public ICommand SendMsgCommand { get; }

        private void ConnectSerialPort()
        {
            if (!IsPortOpened)
            {
                Trace.WriteLine("connecting to serial port");
                spManager.SetPortName(PortName)
                    .SetBaudRate(BaudRate)
                    .SetDataBits(DataBits)
                    .SetParity(Parity)
                    .SetStopBits(StopBits)
                    .OpenPort();
                Trace.WriteLine(spManager.GetSerialPortSettingStr(false));
            }
            else
            {
                Trace.WriteLine("closing serial port");
                spManager.ClosePort();
                Trace.WriteLine(spManager.GetSerialPortSettingStr(false));
            }
        }

        private void SendMsg()
        {
            if (TxInHex)
            {
                spManager.SendData(SerialPortManager.HexStr2ByteArr(TbTxStr));
            }
            else
            {
                spManager.SendData(TbTxStr);
            }
        }

        private void OnComDataReceived(object sender, ComDataReceivedEventArgs e)
        {
            byte[] data = e.DataBytes;
            StringBuilder sb = new StringBuilder();

            sb.Append(RxAutoWrap && TbRxStr != "" ? "\r\n" : "")
                .Append(RxShowDate ? DateTime.Now.ToLocalTime().ToString("[ HH:mm:ss ] ") : "")
                .Append(RxInHex ? SerialPortManager.ByteArr2HexStr(data) : SerialPortManager.ByteArr2AscStr(data));

            TbRxStr += sb;
        }
    }
}
