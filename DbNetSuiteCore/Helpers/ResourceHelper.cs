using System;
using System.IO;
using System.Reflection;

namespace DbNetSuiteCore.Helpers
{
    public static class ResourceHelper
    {
        public static string GetResource(string name)
        {
            var assembly = typeof(ResourceHelper).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream($"DbNetSuiteCore.{name}"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    return $"{text}{System.Environment.NewLine}";
                }
            }
        }

        private static string GetResourceBase64Encoded(string name)
        {
            var assembly = typeof(ResourceHelper).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream($"DbNetSuiteCore.{name}"))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Convert.ToBase64String(bytes); 
            }
        }

        public static string DataUrl(string imageName)
        {
            var content = GetResourceBase64Encoded($"Images.{imageName}.png");
            return $"data:image/png;base64,{content}";
        }
    }
}
