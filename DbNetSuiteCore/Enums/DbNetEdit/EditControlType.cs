using System.ComponentModel;

namespace DbNetSuiteCore.Enums.DbNetEdit
{
    public enum EditControlType
    {
        Auto,
        TextBox,
        TextBoxLookup,
        TextBoxSearchLookup,
        CheckBox,
        DropDownList,
        RadioButtonList,
        TextArea,
        Password,
        Upload,
        SuggestLookup,
        Color,
        Date,
        Email,
        Hidden,
        Month,
        Number,
        Range,
        Tel,
        Time,
        Url,
        Week,
        [Description("datetime-local")]
        DateTime
    }
}