interface DbNetGridEditRequest extends DbNetSuiteRequest {
    columnName: string | undefined;
    delete: boolean;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string | undefined;
    fromPart: string;
    insert: boolean;
    lookupColumnIndex?: number | undefined;
    maxImageHeight: number;
    primaryKey: string | undefined;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    navigation: boolean;
    optimizeForLargeDataset: boolean;
    parentChildRelationship: ParentChildRelationship;
    quickSearch: boolean;
    quickSearchToken: string;
    toolbarButtonStyle: ToolbarButtonStyle;
    uploadMetaData: string;
    uploadMetaDataColumn: string;
}
