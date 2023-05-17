interface DbNetGridRequest
{
    booleanDisplayMode: BooleanDisplayMode;
    columnFilters: Dictionary<String>;
    columnName: string | undefined;
    columns: GridColumnRequest[];
    componentId: string;
    connectionString: string;
    connectionType?: string;
    copy: boolean;
    culture: string;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    export: boolean;
    extension?: string | undefined;
    fixedFilterParams: Dictionary<Object>;
    fixedFilterSql: string | undefined;
    fromPart: string;
    frozenHeader: boolean;
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
    quickSearch: boolean;
    quickSearchToken: string;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    toolbarButtonStyle: ToolbarButtonStyle;
    view: boolean;
}