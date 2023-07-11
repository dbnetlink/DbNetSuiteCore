interface DbNetGridEditRequest extends DbNetSuiteRequest { 
    fromPart: string;
    lookupColumnIndex?: number | undefined;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    navigation: boolean;
    quickSearch: boolean;
    quickSearchToken: string;
}