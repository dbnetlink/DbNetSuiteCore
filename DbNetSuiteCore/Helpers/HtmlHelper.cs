using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbNetSuiteCore.Helpers
{
    public static class HTMLHelpers
    {
        public static IHtmlContent ToolbarIconButton(this IHtmlHelper htmlHelper, string name)
            => new HtmlString($"<button class=\"{name}\" type=\"button\"><img src=\"{ResourceHelper.DataUrl(name)}\"/></button>");

        public static IHtmlContent CellValue(this IHtmlHelper htmlHelper, DataRow row, DataColumn column, List<GridColumn> columns)
        {
            GridColumn gc = columns.FirstOrDefault(c => c.ColumnName == column.ColumnName);
            var value = row[column.ColumnName];

            if (gc == null)
            {
                return new HtmlString(value?.ToString());
            }

            switch (gc.StandardFormat)
            {
                case Format.Email:
                    value = $"<a href=\"mailto:{value}\">{value}</a>";
                    break;
                case Format.Hyperlink:
                    value = $"<a target=\"_blank\" href=\"{value}\">{value}</a>";
                    break;
            }

            if (string.IsNullOrEmpty(gc.Lookup) == false)
            {
                if (gc.LookupData.ContainsKey(value.ToString()))
                {
                    value = gc.LookupData[value.ToString()];
                }
            }

            return new HtmlString(value?.ToString());
        }

        public static IHtmlContent CellHeading(this IHtmlHelper htmlHelper, DataColumn column, List<GridColumn> columns)
        {
            GridColumn gc = columns.FirstOrDefault(c => c.ColumnName == column.ColumnName);
            return new HtmlString($"<span>{(string.IsNullOrEmpty(gc.Label) ? gc.ColumnName : gc.Label)}</span><img src=\"{ResourceHelper.DataUrl("chevron-up")}\"/>");
        }
    }
}
