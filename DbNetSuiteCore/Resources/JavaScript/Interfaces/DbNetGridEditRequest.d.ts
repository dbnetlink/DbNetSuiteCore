interface DbNetGridEditRequest extends DbNetSuiteRequest {
    delete: boolean;
    fromPart: string;
    insert: boolean;
    lookupColumnIndex?: number | undefined;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    navigation: boolean;
    optimizeForLargeDataset: boolean;
    parentChildRelationship: ParentChildRelationship;
    quickSearch: boolean;
    quickSearchToken: string;
}
