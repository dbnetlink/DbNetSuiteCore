using DbNetSuiteCore.Enums;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Html;

namespace DbNetSuiteCore.ViewModels.DbNetFile
{
    public class ToolbarBaseViewModel : BaseViewModel
    {
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool QuickSearch { get; set; }

        public HtmlString Button(string type, ToolbarSection toolbarSection)
        {
            return new HtmlString($"<td class=\"{(toolbarSection == ToolbarSection.Navigation ? "navigation" : "")}\"><button type=\"button\" class=\"toolbar-button toolbar-button-{ToolbarButtonStyle.ToString().ToLower()}\" title=\"{Title(type, toolbarSection)}\" button-type=\"{type.ToLower()}\" id=\"{ComponentId}_{type}Btn\">{(ToolbarButtonStyle == ToolbarButtonStyle.Image ? string.Empty : ResourceString(type))}</button></td>");
        }

        public HtmlString Title(string type, ToolbarSection? toolbarSection = null)
        {
            return new HtmlString(ResourceString($"{type}{(toolbarSection.HasValue && toolbarSection.Value == ToolbarSection.Navigation ? "Row" : string.Empty)}_tooltip"));
        }

        public HtmlString Input(string name)
        {
            return new HtmlString($"<td class=\"navigation\"><input type=\"input\" class=\"toolbar-info\" readonly id=\"{ComponentId}_{name}\" name=\"{name}\"/></td>");
        }

        public HtmlString QuickSearchInput()
        {
            string id = "QuickSearch";
            return new HtmlString($"<td><input type=\"input\" class=\"toolbar-info\" id=\"{ComponentId}_{id}\" name=\"{id}\" title=\"{Title(id)}\"/></td>");
        }

        public HtmlString Text(string text)
        {
            return new HtmlString($"<td class=\"navigation\"><span class=\"toolbar-text\">{ResourceString(text)}</span></td>");
        }

        public HtmlString NoRecords()
        {
            return new HtmlString($"<td id=\"{ComponentId}_no-records-cell\" class=\"no-records\"><span class=\"toolbar-text\">{ResourceString("No_Records_Found")}</span></td>");
        }
        public HtmlString ExportSelect()
        {
            return new HtmlString($"<td><select class=\"toolbar-select\" id=\"{ComponentId}_ExportSelect\"/><option value=\"html\">HTML</option><option value=\"excel\">Excel</option></select</td>");
        }
    }
}
