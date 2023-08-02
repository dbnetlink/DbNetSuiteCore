interface DbNetGridRequest extends DbNetGridEditRequest {
    booleanDisplayMode: BooleanDisplayMode;
    columnFilters: Dictionary<string>;
    columns: GridColumnRequest[];
    copy: boolean;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    export: boolean;
    extension?: string | undefined;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string | undefined;
    frozenHeader: boolean;
    gridGenerationMode: GridGenerationMode;
    groupBy: boolean;
    multiRowSelect: boolean;
    multiRowSelectLocation: MultiRowSelectLocation;
    nestedGrid: boolean;
    orderBy: string;
    orderByDirection: string;
    pageSize: number;
    procedureName: string | undefined;
    procedureParams: Dictionary<object>;
    toolbarButtonStyle: ToolbarButtonStyle;
    update: boolean;
    view: boolean;
    viewLayoutColumns: number;
}
