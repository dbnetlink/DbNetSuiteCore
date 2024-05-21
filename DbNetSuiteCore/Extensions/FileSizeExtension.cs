using DbNetSuiteCore.Enums.DbNetFile;
using System;

namespace DbNetSuiteCore.Extensions
{
    public static class FileSizeExtension
    {
        public static string ToFileSizeUnit(this Int64 value)
        {
            int unit = 1;
            while (Convert.ToDouble(value / Math.Pow(1024, unit)) > 1)
            {
                unit++;
            }

            SizeUnits sizeUnit = (SizeUnits)unit - 1;
            return (value / (double)Math.Pow(1024, (Int64)sizeUnit)).ToString($"0{(sizeUnit == SizeUnits.Byte ? string.Empty : ".00")} {sizeUnit}");
        }
    }
}
