using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Models.Configuration;
using DbNetSuiteCore.Services.Interfaces;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
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

            if (gc == null || value == null)
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

            if (string.IsNullOrEmpty(gc.Format) == false)
            {
                value = FormatValue(value, gc.Format);
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

        public static IHtmlContent CellAttributes(this IHtmlHelper htmlHelper, DataColumn column, List<GridColumn> columns)
        {
            GridColumn gc = columns.FirstOrDefault(c => c.ColumnName == column.ColumnName);
            if (CellAlignment(column.DataType) == HorizontalAlignment.Right && string.IsNullOrEmpty(gc.Lookup))
            {
                return new HtmlString($" class=\"align-right\"");
            }

            return new HtmlString(string.Empty);
        }

        private static string FormatValue(object value, string format)
        {
            if (value == null || value.GetType() == typeof(DBNull))
            {
                return string.Empty;
            }

            switch (value.GetType().ToString())
            {
                case "System.Byte":
                    return Convert.ToByte(value).ToString(format);
                case "System.Int16":
                    return Convert.ToInt16(value).ToString(format);
                case "System.Int32":
                    return Convert.ToInt32(value).ToString(format);
                case "System.Int64":
                    return Convert.ToInt64(value).ToString(format);
                case "System.Decimal":
                    return Convert.ToDecimal(value).ToString(format);
                case "System.Single":
                    return Convert.ToSingle(value).ToString(format);
                case "System.Double":
                    return Convert.ToDouble(value).ToString(format);
                case "System.DateTime":
                    return Convert.ToDateTime(value).ToString(format);
                default:
                    return Convert.ToString(value);
            }
        }

        private static HorizontalAlignment CellAlignment(Type type)
        {
            switch (type.ToString())
            {
                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Decimal":
                case "System.Single":
                case "System.Double":
                case "System.DateTime":
                    return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }
}
