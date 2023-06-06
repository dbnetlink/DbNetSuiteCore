using AngleSharp.Dom;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace DbNetSuiteCore.Tests.Extensions
{
    public static class AngleSharpExtensions
    {
        public static string NameAttr(this IElement element)
        {
            return AttrValue(element, "name");
        }

        public static string StyleAttr(this IElement element)
        {
            return AttrValue(element, "style");
        }

        public static string TypeAttr(this IElement element)
        {
            return AttrValue(element, "type");
        }
        public static string AttrValue(IElement element, string name)
        {
            return element.Attributes[name]!.Value.ToString();
        }
    }
}
