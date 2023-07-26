interface DbNetGridEditRequest extends DbNetSuiteRequest {
    delete: boolean;
    fromPart: string;
    insert: boolean;
    lookupColumnIndex?: number | undefined;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    navigation: boolean;
    quickSearch: boolean;
    quickSearchToken: string;
    parentChildRelationship: ParentChildRelationship;
}
