using System;
using System.Text;

namespace DbNetSuiteCore.Helpers
{
    public static class TextHelper
    {
        private static string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        public static string StripBOM(string resource)
        {
            if (resource.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                resource = resource.Remove(0, _byteOrderMarkUtf8.Length);
            }

            return resource;
        }
    }
}
