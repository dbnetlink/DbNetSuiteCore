﻿using System.Text;
using System;
using System.Linq;

namespace DbNetSuiteCore.Helpers
{
    public static class EncodingHelper
    { 
        const byte xorConstant = 0x53;
        public static bool SuppressEncoding = false;

        public static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input) || input == "*" || SuppressEncoding)
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

        public static string Decode(string input)
        {
            if (string.IsNullOrEmpty(input) || input == "*" || SuppressEncoding)
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

        public static Byte XorConstantFromSessionId(string sessionId)
        {
            return Encoding.UTF8.GetBytes(sessionId).First();
        }
    }
}
