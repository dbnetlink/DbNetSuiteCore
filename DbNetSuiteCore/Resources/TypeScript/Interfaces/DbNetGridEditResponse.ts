interface DbNetGridEditResponse extends DbNetSuiteResponse {
    toolbar: string;
    totalRows: number;
    record?: Dictionary<string>;
    searchParams?: Array<SearchParam>;
}