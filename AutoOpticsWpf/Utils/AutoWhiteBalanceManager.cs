using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoOpticsWpf.Models;

namespace AutoOpticsWpf.Utils
{
    internal class AutoWhiteBalanceManager
    {
        private static AutoWhiteBalanceManager? _instance;

        public static AutoWhiteBalanceManager GetInstance()
        {
            _instance ??= new AutoWhiteBalanceManager();
            return _instance;
        }

        private SerialPortManager spManager = SerialPortManager.GetInstance();
        private CaDeviceManager caDeviceManager = CaDeviceManager.GetInstance();

        public double TgtUd { set; get; } // 目标u'值
        public double TgtVd { set; get; } // 目标v'值
        public double TolUd { set; get; } // u'值允许公差范围
        public double TolVd { set; get; } // v'值允许公差范围

        public Dictionary<string, string> CmdDictionary { internal set; get; }


        private AutoWhiteBalanceManager()
        {
            CmdDictionary = new Dictionary<string, string>
            {
                { "HexCmdIncrementOfGainR", "D5 11 11 35 01 AA" },
                { "HexCmdDecrementOfGainR", "D5 11 11 35 00 AA" },
                { "HexCmdIncrementOfGainG", "D5 11 11 36 01 AA" },
                { "HexCmdDecrementOfGainG", "D5 11 11 36 00 AA" },
                { "HexCmdIncrementOfGainB", "D5 11 11 37 01 AA" },
                { "HexCmdDecrementOfGainB", "D5 11 11 37 00 AA" }
            };
            //TgtUd = 0.1935;
            //TgtVd = 0.4586;

            TgtUd = 0.1850;
            TgtVd = 0.4382;
            TolUd = 0.0005;
            TolVd = 0.0005;

        }
        private void SendCommand(string cmdHexStr, int count)
        {
            for (int i = 0; i < count; i++)
            {
                spManager.SendData(SerialPortManager.HexStr2ByteArr(cmdHexStr));
                Thread.Sleep(1000);
            }
        }

        private void SendCommand(string cmdHexStr)
        {
            SendCommand(cmdHexStr, 1);
        }

        public AutoWhiteBalanceManager PerformAutoWhiteBalance(int maxCount)
        {
            ColorCord ccTgt = new ColorCord(0, 0, 0, TgtUd, TgtVd);
            ColorCord ccCur = caDeviceManager.SimpleMeasure();
            double dEuv = ColorCord.CalDeltaEUdVd(ccTgt, ccCur);
            int count = 0;
            Trace.WriteLine("Performing Auto White Balance");
            string[] colorSeq = { "R", "G", "B" };
            bool succeed = true;

            while (Math.Abs(ccCur.Ud - ccTgt.Ud) > TolUd || Math.Abs(ccCur.Vd - ccTgt.Vd) > TolVd)
            {
                foreach (string color  in colorSeq)
                {
                    SendCommand(CmdDictionary["HexCmdDecrementOfGain" + color]);
                    ccCur = caDeviceManager.SimpleMeasure();
                    double dEuvCur = ColorCord.CalDeltaEUdVd(ccTgt, ccCur);

                    int gainToAdjust = dEuvCur < dEuv ? 2 : 1;
                    dEuv = dEuvCur < dEuv ? dEuvCur : dEuv;

                    SendCommand(CmdDictionary["HexCmdIncrementOfGain" + color], 2);
                    ccCur = caDeviceManager.SimpleMeasure();
                    dEuvCur = ColorCord.CalDeltaEUdVd(ccTgt, ccCur);

                    if (dEuvCur < dEuv)
                    {
                        dEuv = dEuvCur;
                    }
                    else
                    {
                        SendCommand(CmdDictionary["HexCmdDecrementOfGain" + color], gainToAdjust);
                    }
                }

                count++;
                Trace.WriteLine(count + " round finished");

                if (count >= maxCount)
                {
                    succeed = false;
                    break;
                }
            }

            StringBuilder sb =
                new StringBuilder(succeed ? "Succeed to get white balance" : "Fail to get white balance");
            sb.AppendLine()
                .Append("Target u': " + ccTgt.Ud.ToString("f4"))
                .Append("; Current u': " + ccCur.Ud.ToString("f4"))
                .Append("; delta u': " + (ccCur.Ud - ccTgt.Ud).ToString("f4"))
                .AppendLine()
                .Append("Target v': " + ccTgt.Vd.ToString("f4"))
                .Append("; Current v': " + ccCur.Vd.ToString("f4"))
                .Append("; delta v': " + (ccCur.Vd - ccTgt.Vd).ToString("f4"));
            Trace.WriteLine(sb);
            return this;
        }
    }
}
