using System;

namespace DbNetSuiteCore.Helpers
{
    public static class BooleanHelper
    {
        const string Yes = "Yes";
        const string No = "No";
        public static string YesNo(string booleanString)
        {
            bool result;

            if (Boolean.TryParse(booleanString.Replace("1",bool.TrueString).Replace("0", bool.FalseString), out result))
            {
                return result ? Yes : No;
            }

            return booleanString;
        }

        public static string Checked(string booleanString)
        {
            return YesNo(booleanString) == Yes ? "Checked" : string.Empty;
        }
    }
}
