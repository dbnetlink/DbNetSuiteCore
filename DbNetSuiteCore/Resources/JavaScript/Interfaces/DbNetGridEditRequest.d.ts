interface DbNetGridEditRequest extends DbNetSuiteRequest {
    columnName: string | undefined;
    delete: boolean;
    fromPart: string;
    insert: boolean;
    lookupColumnIndex?: number | undefined;
    primaryKey: string | undefined;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    navigation: boolean;
    optimizeForLargeDataset: boolean;
    parentChildRelationship: ParentChildRelationship;
    quickSearch: boolean;
    quickSearchToken: string;
}
