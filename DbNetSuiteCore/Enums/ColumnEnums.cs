namespace DbNetSuiteCore.Enums
{
    public enum ColumnPropertyNames
    {
        GroupHeader,
        ClearDuplicateValue
    }

    public enum QueryBuildModes
    {
        Configuration,
        PrimaryKeysOnly,
        FilterListFilter,
        Totals,
        Count,
        Normal,
        Edit,
        View,
        Spreadsheet
    }


    public enum KeyTypes
    {
        PrimaryKey,
        ForeignKey
    }
    public enum EditModes
    {
        Insert,
        Update
    }

    public enum EditControlType
    {
        /// <summary>
        /// Control type is automatically assigned based on the underlying database column
        /// </summary>  
        Auto,
        /// <summary>
        /// Single-line text box
        /// </summary>  
        TextBox,
        /// <summary>
        /// Text box with a lookup button that opens a list selection dialog. Requires the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property to be assigned.
        /// </summary>  
        TextBoxLookup,
        /// <summary>
        /// Text box with a lookup button that opens a list selection dialog that searches data defined by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property to be assigned.
        /// </summary>  
        TextBoxSearchLookup,
        /// <summary>
        /// Forces a field to act as a boolean type
        /// </summary>  
        CheckBox,
        /// <summary>
        /// Edits HTML content with an WYSIWYG HTML editor
        /// </summary>  
        Html,
        /// <summary>
        /// Previews HTML content with button to open a WYSIWYG HTML editor
        /// </summary>  
        HtmlPreview,
        /// <summary>
        /// Drop-down list of values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
        /// </summary>  
        DropDownList,
        /// <summary>
        /// Radio button list of values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
        /// </summary>  
        RadioButtonList,
        /// <summary>
        /// Multi-line list of values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
        /// </summary>  
        ListBox,
        /// <summary>
        /// Displays data in a read-only label
        /// </summary>  
        Label,
        /// <summary>
        /// Multi-line text box
        /// </summary>  
        TextArea,
        /// <summary>
        /// Password field where field contents are obscured
        /// </summary>  
        Password,
        /// <summary>
        /// Binary file upload stored either as a path to a file or in a database BLOB field.
        /// </summary>  
        Upload,
        /// <summary>
        /// Google suggest style lookup searching values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
        /// </summary>  
        SuggestLookup,
        AutoCompleteLookup,
        /// <summary>
        /// Text box with a lookup button that opens a list selection dialog. Requires the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property to be assigned. Allows selection of multiple values assigned as a comma separated list.
        /// </summary>  
        MultiValueTextBoxLookup,
        Selectmenu
    }




    /////////////////////////////////////////////// 
    public enum FilterColumnSelectMode
    /////////////////////////////////////////////// 
    {
        List,
        Input
    }

    public enum FilterColumnModeValues
    {
        Simple,
        Composite,
        Combined
    }

    public enum OrderByDirection
    {
        asc,
        desc
    }
}
