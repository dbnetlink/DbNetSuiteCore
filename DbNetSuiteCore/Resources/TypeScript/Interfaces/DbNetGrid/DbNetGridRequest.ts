interface DbNetGridRequest extends DbNetSuiteRequest { 
    booleanDisplayMode: BooleanDisplayMode;
    columnFilters: Dictionary<string>;
    columnName: string | undefined;
    columns: GridColumnRequest[];
    copy: boolean;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    export: boolean;
    extension?: string | undefined;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string | undefined;
    fromPart: string;
    frozenHeader: boolean;
    gridGenerationMode: GridGenerationMode
    groupBy: boolean;
    lookupColumnIndex?: number | undefined;
    multiRowSelect: boolean;
    multiRowSelectLocation: MultiRowSelectLocation;
    navigation: boolean;
    nestedGrid: boolean;
    optimizeForLargeDataset: boolean;
    orderBy: string;
    orderByDirection: string;
    pageSize: number;
    primaryKey: string | undefined;
    procedureName: string | undefined;
    procedureParams: Dictionary<object>;
    quickSearch: boolean;
    quickSearchToken: string;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    toolbarButtonStyle: ToolbarButtonStyle;
    view: boolean;
}