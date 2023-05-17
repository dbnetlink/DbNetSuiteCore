using System.Text;
using System;

namespace DbNetSuiteCore.Helpers
{
    internal static class EncodingHelper
    { 
        const byte xorConstant = 0x53;
        internal static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input) || input == "*")
            {
                return input;
            }
            byte[] data = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ xorConstant);
            }
            return Convert.ToBase64String(data);
        }

        internal static string Decode(string input)
        {
            if (string.IsNullOrEmpty(input) || input == "*")
            {
                return input;
            }
            byte[] data = Convert.FromBase64String(input);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ xorConstant);
            }
            return Encoding.UTF8.GetString(data);
        }
    }
}
