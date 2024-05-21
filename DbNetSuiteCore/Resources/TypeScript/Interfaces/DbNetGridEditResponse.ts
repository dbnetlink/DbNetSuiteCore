interface DbNetGridEditResponse extends DbNetSuiteResponse {
    totalRows: number;
    record?: Dictionary<object>;
    searchParams?: Array<SearchParam>;
}