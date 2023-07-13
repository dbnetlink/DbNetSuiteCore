interface DbNetGridEditResponse extends DbNetSuiteResponse {
    toolbar: string;
    totalRows: number;
    record?: Dictionary<object>;
    searchParams?: Array<SearchParam>;
}