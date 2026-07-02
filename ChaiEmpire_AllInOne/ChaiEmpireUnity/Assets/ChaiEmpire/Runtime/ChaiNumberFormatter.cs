using System;
using BreakInfinity;

namespace ChaiEmpire
{
    public static class ChaiNumberFormatter
    {
        private static readonly string[] Suffixes =
        {
            "",
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc"
        };

        public static string Rupees(BigDouble value)
        {
            return "Rs " + Compact(value);
        }

        public static string Compact(BigDouble value)
        {
            if (value < 1000 && value > -1000)
            {
                return value.ToDouble().ToString("0.##");
            }

            int suffixIndex = (int)Math.Floor(value.Exponent / 3d);
            if (suffixIndex > 0 && suffixIndex < Suffixes.Length)
            {
                double scaled = value.Mantissa * Math.Pow(10, value.Exponent - suffixIndex * 3);
                return scaled.ToString("0.##") + Suffixes[suffixIndex];
            }

            return value.Mantissa.ToString("0.###") + "e" + value.Exponent;
        }

        public static string PerSecond(BigDouble value)
        {
            return Rupees(value) + "/sec";
        }
    }
}
