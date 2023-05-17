using System.Text.Json.Serialization;

namespace DbNetSuiteCore.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GoogleChartType
    {
        AreaChart,
        BarChart,
        BubbleChart,
        CandlestickChart,
        ColumnChart,
        ComboChart,
        Histogram,
        LineChart,
        PieChart,
        ScatterChart,
        SteppedAreaChart
    }
}