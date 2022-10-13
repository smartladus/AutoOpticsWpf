using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoOpticsWpf.Models;
using CASDK2;

namespace AutoOpticsWpf.Utils
{
    public class CaException : Exception
    {
        public CaException() { }
        public CaException(string message) : base(message) { }
        public CaException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 基础测试数据接收事件参数，用于传递收到亮度和色坐标值
    /// </summary>
    public class SimpMeasDataReceivedEventArgs : EventArgs
    {
        public double Lv { get; internal set; }
        public double Ud { get; internal set; }
        public double Vd { get; internal set; }
        public double Sx { get; internal set; }
        public double Sy { get; internal set; }

        public SimpMeasDataReceivedEventArgs(double lv, double ud, double vd, double sx, double sy)
        {
            Lv = lv;
            Ud = ud;
            Vd = vd;
            Sx = sx;
            Sy = sy;
        }
    }

    internal class CaDeviceManager
    {
        const int NOERR = 0;

        const int MAXPROBE = 10;

        const int MODE_Lvxy = 0;
        const int MODE_Tduv = 1;
        const int MODE_Lvudvd = 5;
        const int MODE_FMA = 6;
        const int MODE_XYZ = 7;
        const int MODE_JEITA = 8;
        const int MODE_LvPeld = 9;
        const int MODE_Waveform = 10;
        const int MODE_FMA2 = 11;
        const int MODE_JEITA2 = 12;
        const int MODE_Waveform2 = 13;

        const int RED = 0;
        const int GREEN = 1;
        const int BLUE = 2;
        const int WHITE = 3;

        private static CaDeviceManager? _instance;

        public static CaDeviceManager GetInstance()
        {
            _instance ??= new CaDeviceManager();
            return _instance;
        }

        private CASDK2Ca200 objCa200;
        private CASDK2Cas objCas;
        private CASDK2Ca objCa;
        private CASDK2Probes objProbes;
        private CASDK2OutputProbes objOutputProbes;
        private CASDK2Probe objProbe;
        private CASDK2Memory objMemory;

        private bool autoConnectFlag = true; // auto or manual
        private bool triggerFinish = true;
        private int err = 0;
        private bool devConnected = false;

        public delegate void SimpMeasDataReceivedHandler(object sender, SimpMeasDataReceivedEventArgs data);

        public event SimpMeasDataReceivedHandler? SimpMeasDataReceived;

        protected void OnSimpMeasDataReceived(object sender, SimpMeasDataReceivedEventArgs data)
        {
            SimpMeasDataReceived?.Invoke(this, data);
        }
        private CaDeviceManager()
        {
            objCa200 = new CASDK2Ca200();
        }

        /// <summary>
        /// 由于SDK里面是每执行一步都会返回一个状态码，在这里统一处理了，只要返回不是0就抛出异常
        /// </summary>
        /// <param name="actionName">操作名称，用于输出日志</param>
        /// <param name="errNum"></param>
        /// <exception cref="CaException"></exception>
        public CaDeviceManager PerformAction(int errNum, string actionName)
        {
            if (errNum != 0)
            {
                string errMsg = "";
                err = GlobalFunctions.CASDK2_GetLocalizedErrorMsgFromErrorCode(0, errNum, ref errMsg);
                throw new CaException(actionName + " failed: " + errMsg);
            }
            Trace.WriteLine(actionName + " succeed");
            return this;
        }

        public CaDeviceManager PerformAction(int errNum)
        {
            return PerformAction(errNum, "Ca Operation");
        }

        public string GetSdkVersion()
        {
            int lA = 0, lB = 0, lC = 0, lDEFG = 0;
            PerformAction(GlobalFunctions.CASDK2_GetVersion(ref lA, ref lB, ref lC, ref lDEFG), "GetSDKVersion");
            string version = lA + "." + lB + lC + "." + lDEFG;
            Trace.WriteLine("SDK Version:" + version);
            return version;
        }

        public CaDeviceManager SetAutoConnectFlag(bool flag)
        {
            autoConnectFlag = flag;
            return this;
        }

        public bool IsDevConnected()
        {
            return devConnected;
        }

        public CaDeviceManager Connect()
        {
            if (autoConnectFlag)
            {
                AutoConnect();
            }
            return this;
        }

        private void AutoConnect()
        {
            PerformAction(objCa200.AutoConnect(), "Auto Connect");
            PerformAction(objCa200.get_SingleCa(ref objCa), "Getting SingleCa");
            PerformAction(objCa.get_Memory(ref objMemory), "Getting Ca memory");
            PerformAction(objCa.get_SingleProbe(ref objProbe), "Getting Single Probe");
            devConnected = true;
        }

        public CaDeviceManager UseDefaultSetting()
        {
            int freqMode = 4;   // SyncMode : INT 
            double freq = 60.0; //frequency = 60.0Hz
            int speed = 3;      //Measurement speed : Auto
            int lvMode = 1;     //Lv : cd/m2

            PerformAction(objCa.CalZero(), "Zero Calibration");                         //Zero-Calibration
            PerformAction(objCa.put_DisplayProbe("P1"), "Setting DisplayProbe");        //Set display probe to P1
            PerformAction(objCa.put_SyncMode(freqMode, freq), "Setting Sync Mode");     //Set sync mode and frequency
            PerformAction(objCa.put_AveragingMode(speed), "Setting Measurement Speed"); //Set measurement speed
            PerformAction(objCa.put_BrightnessUnit(lvMode), "Setting Brightness Unit"); //SetBrightness unit

            string portId = "";
            string dispProbe = "";
            int syncMode = 0;
            double syncFreq = 0.0;
            int measSpeed = 0;

            //Get settings
            PerformAction(objCa.get_PortID(ref portId));                             //Get connection interface
            Trace.WriteLine("PortID:" + portId);
            PerformAction(objCa.get_DisplayProbe(ref dispProbe));                 //Get display probe
            Trace.WriteLine("DisplayProbe:" + dispProbe);
            PerformAction(objCa.get_SyncMode(ref syncMode, ref syncFreq));        //Get sync mode and frequency
            Trace.WriteLine("SyncMode:" + syncMode + ",SyncFreq:" + syncFreq);
            PerformAction(objCa.get_AveragingMode(ref measSpeed));                //Get measurement speed
            Trace.WriteLine("MeasurementSpeed:" + measSpeed);

            return this;
        }

        public CaDeviceManager UseSimpleMeasSetting()
        {
            SetZeroCalEvent();
            PerformAction(objMemory.put_ChannelNO(1), "Setting Calibration Channel");
            PerformAction(objCa.put_DisplayMode(MODE_Lvudvd), "Setting Display Mode");  //Set mode:Color Lvuv
            return this;
        }

        public ColorCord SimpleMeasure()
        {
            double lv = 0.0;
            double sx = 0.0;
            double sy = 0.0;
            double ud = 0.0;
            double vd = 0.0;
            PerformAction(objCa.Measure(), "Performing Simple Optics Measurement");
            PerformAction(objProbe.get_Lv(ref lv), "Getting Lv value");
            PerformAction(objProbe.get_ud(ref ud), "Getting u' value");
            PerformAction(objProbe.get_vd(ref vd), "Getting v' value");
            PerformAction(objProbe.get_sx(ref sx), "Getting x value");
            PerformAction(objProbe.get_sy(ref sy), "Getting y value");

            OnSimpMeasDataReceived(this, new SimpMeasDataReceivedEventArgs(lv, ud, vd, sx, sy));

            return new ColorCord(lv, sx, sy, ud, vd);
        }

        ///<summary>
        ///[Set Zero Calibration event]
        ///This method execute zero calibration when temperature changes significantly
        ///</summary>
        private int ExeCalZero(int dummy)
        {
            PerformAction(objCa.CalZero(), "Setting action for zero calibration event");   //Zero calibration
            return err;
        }

        ///<summary>
        ///[Set Zero Calibration event]
        ///This method set zero calibration event
        ///</summary>
        private void SetZeroCalEvent()
        {
            Func<int, int> funcZeoCal = ExeCalZero;
            PerformAction(objCa.SetExeCalZeroCallback(funcZeoCal), "Set function for zero calibration event");      //Set function for zero calibration event
        }
    }
}
