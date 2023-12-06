using DbNetSuiteCore.Enums.DbNetEdit;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using System.Collections.Generic;
using System;

namespace DbNetSuiteCore.Components
{
    public class DbNetEditCoreColumn : DbNetGridEditCoreColumn
    {
        internal DbNetEditCoreColumn(string[] columnNames, List<ColumnProperty> columnProperties, string fromPart, List<string> columns) : base(columnNames, columnProperties, fromPart, columns)
        {
        }
        /// <summary>
        /// Assigns a foreign key based lookup against a column to provide a descriptive value
        /// </summary>
        public DbNetEditCoreColumn Lookup(Lookup lookup)
        {
            base.Lookup(lookup);
            return this;
        }
        /// <summary>
        /// Assigns an enum based lookup to a column to provide a descriptive value
        /// </summary>
        public new DbNetEditCoreColumn Lookup(Type lookup, bool useNameAsValue = false)
        {
            base.Lookup(lookup, useNameAsValue);
            return this;
        }
        /// <summary>
        /// Assigns a dictionary based lookup to a column to provide a descriptive value
        /// </summary>
        public new DbNetEditCoreColumn Lookup<T>(Dictionary<T, string> lookup)
        {
            base.Lookup(lookup);
            return this; 
        }
        /// <summary>
        /// Creates a lookup based on a list of existing distinct values for the column
        /// </summary>
        public new DbNetEditCoreColumn Lookup()
        {
            base.Lookup();
            return this;
        }
        /// <summary>
        /// Shows/hides the specified column in the control
        /// </summary>
        public new DbNetEditCoreColumn Display(bool display = true)
        {
            base.Display(display);
            return this;
        }
        /// <summary>
        /// Hides the specified column in the control
        /// </summary>
        public new DbNetEditCoreColumn Hidden(bool hide = true)
        { 
            base.Hidden(hide);
            return this;
        }
        /// <summary>
        /// Specifies the column to be shown in the Search dialog
        /// </summary>
        public new DbNetEditCoreColumn Search()
        {
            base.Search();
            return this;
        }
        /// <summary>
        /// Sets a CSS style for the specified column e.g. "background-color:gold; color:steelblue"
        /// </summary>
        public new DbNetEditCoreColumn Style(string style)
        {
            base.Style(style);
            return this;
        }
        /// <summary>
        /// Overrides the default database type for the specified column
        /// </summary>
        public new DbNetEditCoreColumn DataType(Type type)
        {
            base.DataType(type); 
            return this;
        }
        /// <summary>
        /// Sets the column as the foreign key in the linked control
        /// </summary>
        public new DbNetEditCoreColumn ForeignKey()
        {
            base.ForeignKey();
            return this;
        }
        /// <summary>
        /// Sets the display format for the date/numeric column
        /// </summary>
        public new DbNetEditCoreColumn Format(string format)
        { 
            base.Format(format); 
            return this;
        }
        /// <summary>
        /// Sets the label for the column
        /// </summary>
        public new DbNetEditCoreColumn Label(string label)
        { 
            base.Label(label); 
            return this;
        }
        /// <summary>
        /// Indicates the contents of the column contains and should be rendered as a an image e.g. png, jpg
        /// </summary>
        public new DbNetEditCoreColumn Image(ImageConfiguration imageConfiguration)
        {
            base.Image(imageConfiguration);
            return this;
        }
        /// <summary>
        /// Indicates the contents of the column contains should be rendered as a file e.g pdf, xlsx etc
        /// </summary>
        public new DbNetEditCoreColumn File(FileConfiguration fileConfiguration)
        { 
            base.File(fileConfiguration);
            return this;
        }
        /// <summary>
        /// Sets the type of edit control type for the column
        /// </summary>
        public DbNetEditCoreColumn ControlType(EditControlType editControlType)
        {
            SetColumnProperty(ColumnPropertyType.EditControlType, editControlType);
            return this;
        }
        /// <summary>
        /// Sets the size of the edit field
        /// </summary>
        public DbNetEditCoreColumn Size(int size)
        {
            SetColumnProperty(ColumnPropertyType.ColumnSize, size);
            return this;
        }
        /// <summary>
        /// Specifies the columns that will be displayed in the browse dialog
        /// </summary>
        public DbNetEditCoreColumn Browse()
        {
            SetColumnProperty(ColumnPropertyType.Browse, true);
            return this;
        }
        /// <summary>
        /// Indicates the columns that are required in the edit form
        /// </summary>
        public DbNetEditCoreColumn Required()
        {
            SetColumnProperty(ColumnPropertyType.Required, true);
            return this;
        }
        /// <summary>
        /// Disables the ability to modify the field in the form
        /// </summary>
        public DbNetEditCoreColumn ReadOnly()
        {
            SetColumnProperty(ColumnPropertyType.ReadOnly, true);
            return this;
        }

        /// <summary>
        /// Assigns placeholder text for the input field
        /// </summary>
        public DbNetEditCoreColumn Placeholder(string text)
        {
            SetColumnProperty(ColumnPropertyType.Placeholder, text);
            return this;
        }

        /// <summary>
        /// Assigns annotation(help) text to be displayed alognside input field
        /// </summary>
        public DbNetEditCoreColumn Annotation(string text)
        {
            SetColumnProperty(ColumnPropertyType.Annotation, text);
            return this;
        }

        /// <summary>
        /// Assigns input validation properties. Used in conjunction with EditControlType
        /// </summary>
        public DbNetEditCoreColumn Validation(InputValidation inputValidation)
        {
            SetColumnProperty(ColumnPropertyType.InputValidation, inputValidation); 
            return this;
        }

        /// <summary>
        /// Sets the case of the edit input to be uppercase, lowercase or capitalized
        /// </summary>
        public DbNetEditCoreColumn TextTransform(TextTransform textTransform)
        {
            SetColumnProperty(ColumnPropertyType.TextTransform, textTransform);
            return this;
        }
        /// <summary>
        /// Sets the column as the primary key 
        /// </summary>
        public new DbNetEditCoreColumn PrimaryKey()
        {
            base.PrimaryKey();
            return this;
        }
    }
}