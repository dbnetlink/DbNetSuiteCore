using DbNetSuiteCore.Enums;
using GridColumn = DbNetSuiteCore.Models.DbNetGrid.GridColumn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Resources;
using DbNetSuiteCore.Extensions;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.EMMA;
using DbNetSuiteCore.Models.DbNetGrid;

namespace DbNetSuiteCore.ViewModels.DbNetGrid
{
    public class GridViewModel : BaseViewModel
    {
        public List<GridColumn> Columns { get; set; }
        public DataTable GridData { get; set; }
        public bool MultiRowSelect { get; set; }
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; }
        public bool NestedGrid { get; set; }
        public Dictionary<string, DataTable> LookupTables { get; set; }
        public BooleanDisplayMode BooleanDisplayMode { get; set; }
        public Dictionary<string, string> ColumnFilters { get; set; }
        public bool FrozenHeader { get; set; }
        public GridGenerationMode GridGenerationMode { get; set; }
        public bool GroupBy { get; set; }

        public List<GridColumn> GroupHeaderColumns => Columns.Where(c => c.GroupHeader).ToList();
        public List<GridColumn> TotalBreakColumns => Columns.Where(c => c.TotalBreak).ToList();
        public List<GridColumn> FixedOrderColumns => Columns.Where(c => c.FixedOrder).ToList();
        public DataTable GridTotals { get; set; }

        public object ColumnTotal(GridColumn changeColumn, int? rowCount, GridColumn aggregateColumn)
        {
            DataColumn dc = GridTotals.Columns.Cast<DataColumn>().FirstOrDefault(c => c.ColumnName.ToLower() == aggregateColumn.ColumnName.ToLower());

            int aggregateOrdinal = dc.Ordinal;

            var totalRows = GridTotals.Rows.Cast<DataRow>();

            if (changeColumn != null)
            {
                if (rowCount.HasValue)
                {
                    foreach (var breakColumn in TotalBreakColumns)
                    {
                        int? filterColumnIdx = GridTotals.ColumnIndex(breakColumn);
                        int? dataColumnIdx = GridData.ColumnIndex(breakColumn);
                        if (filterColumnIdx.HasValue && dataColumnIdx.HasValue)
                        {
                            var filterValue = GridData.ColumnValue(rowCount.Value - 1,dataColumnIdx.Value)?.ToString();
                            totalRows = totalRows.Where(r =>
                                r.ItemArray[filterColumnIdx.Value]?.ToString() == filterValue);

                            if (changeColumn.ColumnKey == breakColumn.ColumnKey)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            switch (aggregateColumn.Aggregate)
            {
                case AggregateType.Sum:
                    return totalRows.Sum(r => Convert.ToDecimal(r.ItemArray[aggregateOrdinal]));
                case AggregateType.Avg:
                    return totalRows.Average(r => Convert.ToDecimal(r.ItemArray[aggregateOrdinal]));
                case AggregateType.Min:
                    return totalRows.Min(r => r.ItemArray[aggregateOrdinal]);
                case AggregateType.Max:
                    return totalRows.Max(r => r.ItemArray[aggregateOrdinal]);
                default:
                    return totalRows.Count();
            }
        }

        public bool DuplicateValue(GridColumn column, int row)
        {
            if (row == 0)
            {
                return false;
            }

            return GridData.ColumnValue(row, column)?.ToString() == GridData.ColumnValue(row - 1, column)?.ToString();
        }

        public bool FixedOrderChange(int row, bool firstRow)
        {
            if (FixedOrderColumns.Any())
            {
                if (row == 0)
                {
                    return firstRow;
                }
                var previousValue = FixedOrderColumnValue(row - 1);
                var currentValue = FixedOrderColumnValue(row);
                return previousValue != currentValue;
            }
            return false;

            string FixedOrderColumnValue(int idx)
            {
                return string.Join(string.Empty, FixedOrderColumns.Select(c => GridData.Rows[idx][c.Index]?.ToString() ?? string.Empty));
            }
        }

        public bool ColumnChange(GridColumn column, int row)
        {
            return (row > 0) ? ColumnValue(row-1) != ColumnValue(row) : false;

            string ColumnValue(int idx)
            {
                return GridData.Rows[idx][column.Index]?.ToString() ?? string.Empty;
            }
        }
    }
}