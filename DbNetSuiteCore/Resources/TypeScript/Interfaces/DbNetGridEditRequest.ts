interface DbNetGridEditRequest extends DbNetSuiteRequest { 
    fromPart: string;
    search: boolean;
    navigation: boolean;
    quickSearch: boolean;
    quickSearchToken: string;
}