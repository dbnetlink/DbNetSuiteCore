using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DbNetSuiteCore.Helpers
{
    public static class MyHTMLHelpers
    {
        public static IHtmlContent ToolbarIconButton(this IHtmlHelper htmlHelper, string name)
            => new HtmlString($"<button class=\"{name}\" type=\"button\"><img src=\"{ResourceHelper.DataUrl(name)}\"/></button>");
    }
}
