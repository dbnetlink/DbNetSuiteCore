using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Models.Configuration;
using DbNetSuiteCore.Services.Interfaces;
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

        public static IHtmlContent CellHeading(this IHtmlHelper htmlHelper, DataColumn column, DbNetGridConfiguration configuration)
        {
            GridColumn gc = configuration.Columns.FirstOrDefault(c => c.ColumnName == column.ColumnName);
            string heading = $"<span>{(string.IsNullOrEmpty(gc.Label) ? gc.ColumnName : gc.Label)}</span>";

            if (configuration.OrderByColumn == column.ColumnName)
            {
                heading += $"<img sequence=\"{configuration.OrderBySequence}\" src=\"{ResourceHelper.DataUrl($"chevron-{(configuration.OrderBySequence == "asc" ? "up" : "down")}")}\"/>";
            }
            return new HtmlString(heading);
        }

        public static List<SelectListItem> DropDownListFilter(this IHtmlHelper htmlHelper, DataColumn column, DbNetGridConfiguration configuration)
        {
            GridColumn gc = configuration.Columns.FirstOrDefault(c => c.ColumnName == column.ColumnName);
            if (gc.DropDownFilter == false || string.IsNullOrEmpty(gc.Lookup))
            {
                return null;
            }

            var options = gc.LookupData.Select(a => new SelectListItem { Value = a.Key, Text = a.Value }).OrderBy(s => s.Text).ToList();

            if (column.ColumnName == configuration.DropDownFilterColumn)
            {
                options.Where(o => o.Value == configuration.DropDownFilterValue).ToList().ForEach(o => o.Selected = true);
            }
            return options;
        }

        public static IHtmlContent CellHeadingAttributes(this IHtmlHelper htmlHelper, DataColumn column, List<GridColumn> columns)
        {
            GridColumn gc = columns.FirstOrDefault(c => c.ColumnName == column.ColumnName);
            return new HtmlString($" column-name=\"{gc.ColumnName}\"");
        }
    }
}
