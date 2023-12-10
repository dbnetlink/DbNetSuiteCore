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
    public enum EditMode
    {
        Insert,
        Update,
        Delete
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
