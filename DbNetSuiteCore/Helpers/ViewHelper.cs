namespace DbNetSuiteCore.Helpers
{
    public static class ViewHelper
    {
        public static string SetAttribute(string attrName, string attrValue)
        {
            if (string.IsNullOrEmpty(attrValue))
            {
                return string.Empty;
            }

            return $"{attrName}=\"{attrValue}\"";
        }
    }
}