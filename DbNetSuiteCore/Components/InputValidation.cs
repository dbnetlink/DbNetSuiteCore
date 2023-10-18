namespace DbNetSuiteCore.Components
{
    public class InputValidation
    {
        /// <summary>
        /// Is a value in the input field required
        /// </summary>
        public bool? Required { get; set; }
        /// <summary>
        /// The minimum allowed value for a range or number type input
        /// </summary>
        public int? Min { get; set; }
        /// <summary>
        /// The maximum allowed value for a range or number type input
        /// </summary>
        public int? Max { get; set; }
        /// <summary>
        /// The step increment for a range or number type input
        /// </summary>
        public int? Step { get; set; }
        /// <summary>
        /// The miniumum number of characters to be entered in an input
        /// </summary>
        public int? MinLength { get; set; }
        /// <summary>
        /// The maximum number of characters to be entered in an input
        /// </summary>
        public int? MaxLength { get; set; }
        /// <summary>
        /// A regular expression style validation for a text input
        /// </summary>
        public string Pattern { get; set; }
        public InputValidation()
        {
        }
        public InputValidation(bool required)
        {
            Required = required;
        }
        public InputValidation(string pattern)
        {
            Pattern = pattern;
        }
        public InputValidation(int min, int max, int step = 1)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}