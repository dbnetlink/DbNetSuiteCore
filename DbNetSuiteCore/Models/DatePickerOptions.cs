using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading;

namespace DbNetSuiteCore.Models
{
    public class DatePickerOptions
    {
        private readonly CultureInfo _culture;
        public DatePickerOptions(string culture = null)
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            if (string.IsNullOrEmpty(culture) == false)
            {
                _culture = new CultureInfo(culture);
            }
        }
        public bool ChangeMonth { get; set; } = true;
        public bool ChangeYear { get; set; } = true;
        public string YearRange { get; set; } = "c-99:c+10";
        public string CloseText { get; set; } = string.Empty;
        public string PrevText { get; set; } = string.Empty;
        public string NextText { get; set; } = string.Empty;
        public string CurrentText { get; set; } = string.Empty;
        public string[] MonthNames => _culture.DateTimeFormat.MonthNames;
        public string[] MonthNamesShort => _culture.DateTimeFormat.AbbreviatedMonthNames;
        public string[] DayNames => _culture.DateTimeFormat.DayNames;
        public string[] DayNamesMin => _culture.DateTimeFormat.AbbreviatedDayNames;
        public string DateFormat => _culture.DateTimeFormat.ShortDatePattern.ToLower().Replace("yyyy", "yy");
        [JsonPropertyName("isRTL")]
        public bool IsRtl => _culture.TextInfo.IsRightToLeft;
        public int FirstDay => 1;
        public string ShowOn { get; set; } = "none";
        public bool ConstrainInput => false;
    }
}

