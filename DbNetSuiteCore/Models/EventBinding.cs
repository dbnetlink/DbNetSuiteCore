using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.Models
{
    internal class EventBinding
    {
        public EventType EventType { get; set; }
        public string FunctionName { get; set; }

        public EventBinding(EventType eventType, string functionName) 
        { 
            EventType = eventType;
            FunctionName = functionName;
        }
    }
}
