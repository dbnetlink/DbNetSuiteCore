using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridCoreColumn : DbNetGridEditCoreColumn
    {
        internal DbNetGridCoreColumn(string[] columnNames, List<ColumnProperty> columnProperties, string fromPart, List<string> columns) : base(columnNames, columnProperties, fromPart, columns)
        {
        }
        /// <summary>
        /// Assigns a foreign key based lookup against a column to provide a descriptive value
        /// </summary>
        public DbNetGridCoreColumn Lookup(Lookup lookup)
        {
            base.Lookup(lookup);
            return this;
        }
        /// <summary>
        /// Assigns an enum based lookup against a column to provide a descriptive value
        /// </summary>
        public new DbNetGridCoreColumn Lookup(Type lookup, bool useNameAsValue = false)
        {
            base.Lookup(lookup, useNameAsValue);
            return this;
        }
        /// <summary>
        /// Creates a lookup based on a list of existing distinct values for the column
        /// </summary>
        public new DbNetGridCoreColumn Lookup()
        {
            base.Lookup();
            return this;
        }
        /// <summary>
        /// Shows/hides the specified column in the control
        /// </summary>
        public new DbNetGridCoreColumn Display(bool display = true)
        {
            base.Display(display);
            return this;
        }
        /// <summary>
        /// Hides the specified column in the control
        /// </summary>
        public new DbNetGridCoreColumn Hidden(bool hide = true)
        {
            base.Hidden(hide);
            return this;
        }
        /// <summary>
        /// Specifies the column to be shown in the Search dialog
        /// </summary>
        public new DbNetGridCoreColumn Search()
        {
            base.Search();
            return this;
        }
        /// <summary>
        /// Sets a CSS style for the specified column e.g. "background-color:gold; color:steelblue"
        /// </summary>
        public new DbNetGridCoreColumn Style(string style)
        {
            base.Style(style);
            return this;
        }
        /// <summary>
        /// Overrides the default database type for the specified column
        /// </summary>
        public new DbNetGridCoreColumn DataType(Type type)
        {
            base.DataType(type);
            return this;
        }
        /// <summary>
        /// Sets the column as the foreign key in the linked control
        /// </summary>
        public new DbNetGridCoreColumn ForeignKey()
        {
            base.ForeignKey();
            return this;
        }
        /// <summary>
        /// Sets the display format for the date/numeric column
        /// </summary>
        public new DbNetGridCoreColumn Format(string format)
        {
            base.Format(format);
            return this;
        }
        /// <summary>
        /// Sets the label for the column
        /// </summary>
        public new DbNetGridCoreColumn Label(string label)
        {
            base.Label(label);
            return this;
        }
        /// <summary>
        /// Indicates the contents of the column contains and should be rendered as a an image e.g. png, jpg
        /// </summary>
        public new DbNetGridCoreColumn Image(ImageConfiguration imageConfiguration)
        {
            base.Image(imageConfiguration);
            return this;
        }
        /// <summary>
        /// Indicates the contents of the column contains should be rendered as a file e.g pdf, xlsx etc
        /// </summary>
        public new DbNetGridCoreColumn File(FileConfiguration fileConfiguration)
        {
            base.File(fileConfiguration);
            return this;
        }
        /// <summary>
        /// Specifies the column that should have duplicate adjacent values cleared for readability.
        /// </summary>
        public DbNetGridCoreColumn ClearDuplicateValue()
        {
            SetColumnProperty(ColumnPropertyType.ClearDuplicateValue, true);
            return this;
        }

        /// <summary>
        /// Specifies the column that should trigger a summary aggregatre when the value changes.
        /// </summary>
        public DbNetGridCoreColumn TotalBreak()
        {
            SetColumnProperty(ColumnPropertyType.TotalBreak, true);
            return this;
        }
        /// <summary>
        /// Specifies the grid column should not be displayed but the value stored as an attribute of the row.
        /// </summary>
        public DbNetGridCoreColumn DataOnly()
        {
            SetColumnProperty(ColumnPropertyType.DataOnly, true);
            return this;
        }

        /// <summary>
        /// Specifies the grid column that should have a filter. Use "*" for all columns.
        /// </summary>
        public DbNetGridCoreColumn Filter()
        {
            SetColumnProperty(ColumnPropertyType.Filter, true);
            return this;
        }

        /// <summary>
        /// Specifies the style of the column filter for the column.
        /// </summary>
        public DbNetGridCoreColumn FilterMode(FilterMode filterMode)
        {
            SetColumnProperty(ColumnPropertyType.FilterMode, filterMode);
            return this;
        }


        /// <summary>
        /// Specifies the type of aggregate for an aggregated column.
        /// </summary>
        public DbNetGridCoreColumn Aggregate(AggregateType aggregateType)
        {
            SetColumnProperty(ColumnPropertyType.Aggregate, aggregateType);
            return this;
        }

        /// <summary>
        /// Specifies the column to be shown in the View dialog. Use "*" for all columns.
        /// </summary>
        public DbNetGridCoreColumn View()
        {
            SetColumnProperty(ColumnPropertyType.View, true);
            return this;
        }

        /// <summary>
        /// Specifies the columns to be shown in the View dialog
        /// </summary>
        public DbNetGridCoreColumn GroupHeader()
        {
            SetColumnProperty(ColumnPropertyType.GroupHeader, true);
            return this;
        }
        /// <summary>
        /// Sets the column as the primary key 
        /// </summary>
        public new DbNetGridCoreColumn PrimaryKey()
        {
            base.PrimaryKey();
            return this;
        }
    }
}