using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoOpticsWpf.Models
{
    internal class ColorCord
    {
        public static double CalDeltaEUdVd(ColorCord cc1, ColorCord cc2)
        {
            return Math.Sqrt(Math.Pow(cc1.Ud - cc2.Ud, 2) + Math.Pow(cc1.Vd - cc2.Vd, 2));
        }
        public double Lv { set; get; }
        public double Sx { set; get; }
        public double Sy { set; get; }
        public double Ud { set; get; }
        public double Vd { set; get; }

        public ColorCord(double lv, double sx, double sy, double ud, double vd)
        {
            Lv = lv;
            Sx = sx;
            Sy = sy;
            Ud = ud;
            Vd = vd;
        }
    }
}
