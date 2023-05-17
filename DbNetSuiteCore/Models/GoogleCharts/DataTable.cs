using System.Collections.Generic;

namespace DbNetSuiteCore.Models.GoogleCharts
{
    public class DataTable
    {
        public List<Col> Cols { get; set; } = new List<Col>();
        public List<Row> Rows { get; set; } = new List<Row>();
    }

    public class Col
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
    }

    public class Row
    {
        public List<Cell> C { get; set; } = new List<Cell>();
    }

    public class Cell
    {
        public object V { get; set; }
    }
}
/*

{
    cols: [{ id: 'A', label: 'NEW A', type: 'string'},
    { id: 'B', label: 'B-label', type: 'number'},
    { id: 'C', label: 'C-label', type: 'date'}
    ],
    rows:
    [{
        c: [{ v: 'a'},
        { v: 1.0, f: 'One'},
        { v: new Date(2008, 1, 28, 0, 31, 26), f: '2/28/08 12:31 AM'}
        ]},
    {
        c: [{ v: 'b'},
        { v: 2.0, f: 'Two'},
        { v: new Date(2008, 2, 30, 0, 31, 26), f: '3/30/08 12:31 AM'}
        ]},
    {
        c: [{ v: 'c'},
        { v: 3.0, f: 'Three'},
        { v: new Date(2008, 3, 30, 0, 31, 26), f: '4/30/08 12:31 AM'}
        ]}
    ]
}
*/