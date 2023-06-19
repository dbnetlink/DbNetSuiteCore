using System;

namespace DbNetSuiteCore.Models
{
    internal class EventBinding
    {
        public Enum EventType { get; set; }
        public string FunctionName { get; set; }

        public EventBinding(Enum eventType, string functionName) 
        { 
            EventType = eventType;
            FunctionName = functionName;
        }
    }
}
