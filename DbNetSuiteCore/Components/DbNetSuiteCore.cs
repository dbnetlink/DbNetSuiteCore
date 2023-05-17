using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class DbNetSuiteCore
    {
        public static HtmlString StyleSheet(string fontSize = null, string fontFamily = null)
        {
            List<string> url = new List<string>() { "~/resource.dbnetsuite?action=css" };
            if (string.IsNullOrEmpty(fontSize) == false)
            {
                url.Add($"font-size={fontSize}");
            }
            if (string.IsNullOrEmpty(fontFamily) == false)
            {
                url.Add($"font-family={fontFamily}");
            }

            return new HtmlString($"<link href=\"{string.Join("&", url)}\" type=\"text/css\" rel=\"stylesheet\" />");
        }

        public static HtmlString ClientScript()
        {
            return new HtmlString($"<script src=\"~/resource.dbnetsuite?action=script\"></script>");
        }
    }
}


