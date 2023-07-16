﻿interface DbNetGridRequest extends DbNetGridEditRequest { 
    booleanDisplayMode: BooleanDisplayMode;
    columnFilters: Dictionary<string>;
    columnName: string | undefined;
    columns: GridColumnRequest[];
    copy: boolean;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    delete: boolean;
    export: boolean;
    extension?: string | undefined;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string | undefined;
    frozenHeader: boolean;
    gridGenerationMode: GridGenerationMode
    groupBy: boolean;
    insert: boolean;
    multiRowSelect: boolean;
    multiRowSelectLocation: MultiRowSelectLocation;
    nestedGrid: boolean;
    optimizeForLargeDataset: boolean;
    orderBy: string;
    orderByDirection: string;
    pageSize: number;
    primaryKey: string | undefined;
    procedureName: string | undefined;
    procedureParams: Dictionary<object>;
    toolbarButtonStyle: ToolbarButtonStyle;
    update: boolean;
    view: boolean;
    viewLayoutColumns: number;
}