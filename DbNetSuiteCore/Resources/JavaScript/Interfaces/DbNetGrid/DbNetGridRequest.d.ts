interface DbNetGridRequest extends DbNetGridEditRequest {
    booleanDisplayMode: BooleanDisplayMode;
    columnFilters: Dictionary<string>;
    columns: GridColumnRequest[];
    copy: boolean;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    export: boolean;
    extension?: string | undefined;
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
    update: boolean;
    view: boolean;
    viewLayoutColumns: number;
    exportExtension: string;
    jsonKey: string;
}
