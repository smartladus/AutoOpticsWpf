using System;
using System.Diagnostics;
using System.Text;
using System.IO.Ports;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoOpticsWpf.Utils
{
    /// <summary>
    /// 串口通信异常
    /// </summary>
    public class SerialPortException : Exception
    {
        public SerialPortException(string msg) : base(msg)
        { }
    }

    public class SerialPortStatusChangedMessage : ValueChangedMessage<bool>
    {
        public SerialPortStatusChangedMessage(bool connected) : base(connected)
        { }
    }

    /// <summary>
    /// 串口接收事件参数，用于传递收到的byte[]
    /// </summary>
    public class ComDataReceivedEventArgs : EventArgs
    {
        public byte[] DataBytes { get; internal set; }

        public ComDataReceivedEventArgs(byte[] dataBytes)
        {
            DataBytes = dataBytes;
        }
    }

    public class SerialPortManager
    {
        private static SerialPortManager? _instance;
        public static SerialPortManager GetInstance()
        {
            _instance ??= new SerialPortManager();
            return _instance;
        }

        public static string ByteArr2AscStr(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static string ByteArr2HexStr(byte[] data)
        {
            StringBuilder sb = new();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("X2") + ' ');
            }
            return sb.ToString();
        }

        public static byte[] HexStr2ByteArr(string data)
        {
            string[] hexStrArr = data.Split(" ");
            byte [] bytes = new byte[hexStrArr.Length];
            for (int i = 0; i < hexStrArr.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexStrArr[i], 16);
            }
            return bytes;
        }

        private SerialPort _serialPort;

        /// <summary>
        /// 定义一个串口数据接收事件委托
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="data">事件参数</param>
        public delegate void ComDataReceivedHandler(object sender, ComDataReceivedEventArgs data);

        /// <summary>
        /// 定义一个串口数据接收事件
        /// 外部使用时，[对象名].ComDataReceived += [ComDataReceivedHandler对象]即可注册事件
        /// </summary>
        public event ComDataReceivedHandler? ComDataReceived;

        /// <summary>
        /// 定义串口数据接收事件触发的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        protected void OnComDataReceived(object sender, ComDataReceivedEventArgs data)
        {
            ComDataReceived?.Invoke(this, data);
        }

        private SerialPortManager()
        {
            _serialPort = new SerialPort();
            _serialPort.DataReceived += OnSerialDataReceived;
        }

        public string[] GetSerialPortNameArr()
        {
            return SerialPort.GetPortNames();
        }

        public bool IsPortOpen()
        {
            return _serialPort.IsOpen;
        }

        private void CheckSerialPortObjAvailability(string msg)
        {
            if (IsPortOpen())
            {
                throw new SerialPortException("Fail to " + msg + ", port is already opened");
            }
        }

        public SerialPortManager OpenPort()
        {
            CheckSerialPortObjAvailability("open serial port");
            _serialPort.Open();
            WeakReferenceMessenger.Default.Send(new SerialPortStatusChangedMessage(IsPortOpen()));
            return this;
        }

        public SerialPortManager ClosePort()
        {
            _serialPort.Close();
            WeakReferenceMessenger.Default.Send(new SerialPortStatusChangedMessage(IsPortOpen()));
            return this;
        }

        public SerialPortManager SetPortName(string portName)
        {
            CheckSerialPortObjAvailability("set port name");
            _serialPort.PortName = portName;
            return this;
        }

        public SerialPortManager SetBaudRate(int baudRate)
        {
            CheckSerialPortObjAvailability("set baud rate");
            _serialPort.BaudRate = baudRate;
            return this;
        }

        public SerialPortManager SetParity(Parity parity)
        {
            CheckSerialPortObjAvailability("set parity");
            _serialPort.Parity = parity;
            return this;
        }

        public SerialPortManager SetStopBits(StopBits stopBits)
        {
            CheckSerialPortObjAvailability("set stop bits");
            _serialPort.StopBits = stopBits;
            return this;
        }

        public SerialPortManager SetDataBits(int dataBits)
        {
            CheckSerialPortObjAvailability("set data bits");
            _serialPort.DataBits = dataBits;
            return this;
        }

        public string GetSerialPortSettingStr(bool isSimple)
        {
            StringBuilder sb = new StringBuilder();
            if (isSimple)
            {
                sb.Append(_serialPort.PortName)
                    .Append(" ")
                    .Append(_serialPort.IsOpen ? "OPENED" : "CLOSED")
                    .Append(", ")
                    .Append(_serialPort.BaudRate)
                    .Append(", ")
                    .Append(_serialPort.Parity)
                    .Append(", ")
                    .Append(_serialPort.DataBits)
                    .Append(", ")
                    .Append(_serialPort.StopBits);
            }
            else
            {
                sb.Append("Status: ")
                    .Append(_serialPort.IsOpen ? "OPENED" : "CLOSED")
                    .Append(" ;Port Name: ")
                    .Append(_serialPort.PortName)
                    .Append(" ;Baud Rate: ")
                    .Append(_serialPort.BaudRate)
                    .Append(" ;Parity: ")
                    .Append(_serialPort.Parity)
                    .Append(" ;DataBits: ")
                    .Append(_serialPort.DataBits)
                    .Append(" ;StopBits: ")
                    .Append(_serialPort.StopBits);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 串口发送信息（字符串形式）
        /// </summary>
        /// <param name="data">字符串信息</param>
        /// <param name="hasCr">是否自动添加回车键</param>
        /// <returns></returns>
        public SerialPortManager SendData(string data, bool hasCr)
        {
            if (hasCr)
            {
                _serialPort.WriteLine(data);
            }
            else
            {
                _serialPort.Write(data);
            }
            return this;
        }

        /// <summary>
        /// 串口发送信息（字符串形式），默认自动添加回车键
        /// </summary>
        /// <param name="data">字符串信息</param>
        /// <returns></returns>
        public SerialPortManager SendData(string data)
        {
            return SendData(data, false);
        }

        /// <summary>
        /// 串口发送信息（byte数组形式，16进制）
        /// </summary>
        /// <param name="data">byte数组数据</param>
        /// <returns></returns>
        public SerialPortManager SendData(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
            return this;
        }

        /// <summary>
        /// 串口收到数据时的事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[_serialPort.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            Trace.WriteLine("Data received on port: " + _serialPort.PortName);
            Trace.WriteLine("ASCII: " + ByteArr2AscStr(data));
            Trace.WriteLine("HEX  : " + ByteArr2HexStr(data));
            // 数据接收时触发OnDataReceived，并提供data，供外部调用
            OnComDataReceived(this, new ComDataReceivedEventArgs(data));
        }

    }
}
